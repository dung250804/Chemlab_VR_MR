using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.serializer;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine.formula
{
    public abstract class FormulaHelper
    {
        public static void AddAtomToStructure(Atom rootAtom, Atom addedAtom, Dictionary<Atom, List<Bond>> mutableStructure, Bond.BondType bondType)
        {
            if (!mutableStructure.ContainsKey(rootAtom))
            {
                mutableStructure[rootAtom] = new List<Bond>();
            }
            mutableStructure.Add(addedAtom, new List<Bond>());
            mutableStructure[rootAtom].Add(new Bond(rootAtom, addedAtom, bondType));
            mutableStructure[addedAtom].Add(new Bond(addedAtom, rootAtom, bondType));
        }
        
        public static void AddBondToStructure(Atom srcAtom, Atom dstAtom, Dictionary<Atom, List<Bond>> mutableStructure, Bond.BondType bondType)
        {
            if (!mutableStructure.ContainsKey(srcAtom))
            {
                mutableStructure[srcAtom] = new List<Bond>();
            }
            if (!mutableStructure.ContainsKey(dstAtom))
            {
                mutableStructure[dstAtom] = new List<Bond>();
            }
            mutableStructure[srcAtom].Add(new Bond(srcAtom, dstAtom, bondType));
            mutableStructure[dstAtom].Add(new Bond(dstAtom, srcAtom, bondType));
        }
        
        public static double GetAvailableConnections(Atom atom, IReadOnlyList<Bond> bonds)
        {
            var connections = atom.GetMaxConnectivity();
            connections = bonds.Aggregate(connections, (current, bond) => current -
                                                                          (bond.GetBondType() == Bond.BondType.Aromatic
                                                                              ? 1.5f
                                                                              : (float) bond.GetBondType()));
            connections += atom.FormalCharge;
            return connections;
        }

        public static double GetTotalConnections(Atom atom, IReadOnlyList<Bond> bonds)
        {
            return atom.GetMaxConnectivity() - GetAvailableConnections(atom, bonds);
        }
        
        public static Branch GetMaximumBranch(Atom startAtom, Dictionary<Atom, List<Bond>> structure)
        {
            Dictionary<Atom, Node> allNodes = new();
            foreach (var atom in structure.Keys)
            {
                allNodes[atom] = new Node(atom);
            }

            var currentNode = allNodes[startAtom];
            currentNode.Visited = true;
            var maximumBranch = new Branch(currentNode);
            bool nodesAdded = true;

            while (true)
            {
                while (nodesAdded)
                {
                    nodesAdded = false;
                    Dictionary<Node, Bond.BondType> connectedUnvisitedNodesAndTheirBondTypes = new();
                    foreach (var bond in structure[currentNode.GetAtom()])
                    {
                        var node = allNodes[bond.GetDestinationAtom()];
                        if (node is { Visited: false })
                        {
                            connectedUnvisitedNodesAndTheirBondTypes[node] = bond.GetBondType();
                        }
                    }

                    if (connectedUnvisitedNodesAndTheirBondTypes.Count == 1)
                    {
                        var onlyNode = connectedUnvisitedNodesAndTheirBondTypes.Keys.First();
                        maximumBranch.Add(onlyNode, connectedUnvisitedNodesAndTheirBondTypes[onlyNode]);
                        currentNode = onlyNode;
                        nodesAdded = true;
                    }
                    else if (connectedUnvisitedNodesAndTheirBondTypes.Count() != 0)
                    {
                        Dictionary<Branch, Bond.BondType> connectedBranchesAndTheirBondTypes = new();
                        foreach (var node in connectedUnvisitedNodesAndTheirBondTypes.Keys)
                        {
                            var newStructure = ShallowCopyStructure(structure);
                            Bond bondToRemove = null;
                            foreach (var bond in structure[node.GetAtom()]
                                         .Where(bond => bond.GetDestinationAtom() == currentNode.GetAtom()))
                            {
                                bondToRemove = bond;
                            }

                            if (bondToRemove != null)
                            {
                                newStructure[node.GetAtom()].Remove(bondToRemove);
                            }

                            newStructure.Remove(currentNode.GetAtom());
                            var branch = GetMaximumBranch(node.GetAtom(), newStructure);
                            connectedBranchesAndTheirBondTypes[branch] = connectedUnvisitedNodesAndTheirBondTypes[node];
                        }

                        List<Branch> orderedConnectedBranches = new(connectedBranchesAndTheirBondTypes.Keys);
                        orderedConnectedBranches.Sort((b1, b2) => b2.GetMass().CompareTo(b1.GetMass()));
                        var biggestBranch = orderedConnectedBranches[0];
                        maximumBranch.Add(biggestBranch, connectedBranchesAndTheirBondTypes[biggestBranch]);
                        orderedConnectedBranches.RemoveAt(0);
                        foreach (var sideBranch in orderedConnectedBranches)
                        {
                            currentNode.AddSideBranch(sideBranch, connectedBranchesAndTheirBondTypes[sideBranch]);
                        }
                    }
                }

                return maximumBranch;
            }
        }
        
        public static Branch GetMaximumBranchWithHighestMass(Dictionary<Atom, List<Bond>> structure)
        {
            List<Atom> terminalAtoms = new ();
            if (structure.Count == 1)
            {
                return GetMaximumBranch(structure.Keys.First(), structure);
            }
            foreach (var atom in structure.Keys) {
                if (structure[atom].Count == 1) {
                    terminalAtoms.Add(atom);
                }
            }
            terminalAtoms.Sort((a1, a2) =>
                GetMaximumBranch(a2, structure).GetMassOfLongestChain()
                    .CompareTo(GetMaximumBranch(a1, structure).GetMassOfLongestChain())
            );
            terminalAtoms.Sort((a1, a2) =>
                Branch.GetMassForComparisonInSerialization(a1)
                    .CompareTo(Branch.GetMassForComparisonInSerialization(a2))
            );
            return GetMaximumBranch(terminalAtoms[0], structure);
        }

        public static Dictionary<Atom, List<Bond>> ShallowCopyStructure(Dictionary<Atom, List<Bond>> structureToCopy)
        {
            Dictionary<Atom, List<Bond>> newStructure = new();
            foreach (var atom in structureToCopy.Keys)
            {
                var oldBonds = structureToCopy[atom];
                var newBonds = oldBonds.Select(oldBond =>
                    new Bond(atom, oldBond.GetDestinationAtom(), oldBond.GetBondType())).ToList();

                newStructure[atom] = newBonds;
            }

            return newStructure;
        }

        public static Dictionary<Atom, List<Bond>> StripHydrogens(Dictionary<Atom, List<Bond>> structure)
        {
            Dictionary<Atom, List<Bond>> newStructure = new();
            foreach (var (atom, value) in structure)
            {
                var bondsToInclude = new List<Bond>();
                var includeAtom = !atom.IsNeutralHydrogen();
                foreach (var bond in value) {
                    if (atom.FormalCharge != 0 || bond.GetDestinationAtom().FormalCharge != 0 || !bond.GetDestinationAtom().IsNeutralHydrogen()) {
                        bondsToInclude.Add(bond);
                        if (bond.GetDestinationAtom().FormalCharge != 0) includeAtom = true; // If we're a hydrogen bonded to a charged Atom, include
                    }
                }

                if (includeAtom)
                {
                    newStructure[atom] = bondsToInclude;
                }
            }
            return newStructure;
        }

        public static Bond.BondType TrailingBondType(string symbol)
        {
            return BondSerialize.Deserialize(symbol[^1]);
        }
    }
}