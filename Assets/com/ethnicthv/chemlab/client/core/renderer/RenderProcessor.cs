using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.core.renderer.type;
using com.ethnicthv.chemlab.client.model;
using com.ethnicthv.chemlab.client.model.bond;
using com.ethnicthv.chemlab.client.model.position;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.molecule.formula;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.renderer
{
    public class RenderProcessor
    {
        private readonly PositionCalculator _calculator = new();
        
        private readonly LinkedList<(IFormula, Vector3)> _storageFormulas = new();

        private readonly Dictionary<Element, List<GenericAtomModel>> _atoms = new();
        private readonly List<SingleBondModel> _1Bonds = new();
        private readonly List<DoubleBondModel> _2Bonds = new();
        private readonly List<TripleBondModel> _3Bonds = new();
        
        private readonly List<(Vector3, Vector3)> _bounds = new();
        
        private int _atomCount = 0;
        
        public void ForeachElement(Action<Element, RenderAtomRenderable> action)
        {
            foreach (var temp in _atoms)
            {
                var (element, atoms) = temp;
                action(element, new RenderAtomRenderable(atoms));
            }
        }
        
        public void ForeachSingleBond(Action<SingleBondModel> action)
        {
            foreach (var bond in _1Bonds)
            {
                action(bond);
            }
        }
        
        public void ForeachDoubleBond(Action<DoubleBondModel> action)
        {
            foreach (var bond in _2Bonds)
            {
                action(bond);
            }
        }
        
        public void ForeachTripleBond(Action<TripleBondModel> action)
        {
            foreach (var bond in _3Bonds)
            {
                action(bond);
            }
        }

        public void AddFormula(IFormula formula, Vector3 offset)
        {
            _storageFormulas.AddLast((formula, offset));
            Refresh();
        }

        public void RemoveFormula(IFormula formula)
        {
            var node = _storageFormulas.First;
            while (node != null)
            {
                if (node.Value.Item1 == formula)
                {
                    _storageFormulas.Remove(node);
                    break;
                }

                node = node.Next;
            }

            Refresh();
        }

        public void Clear()
        {
            _storageFormulas.Clear();
        }

        public void Refresh()
        {
            _atoms.Clear();
            _1Bonds.Clear();
            _2Bonds.Clear();
            _3Bonds.Clear();
            _bounds.Clear();
        }
        
        public bool HasAnyRenderEntity()
        {
            return _storageFormulas.Count > 0;
        }

        public (Vector3, Vector3) GetBound(int index)
        {
            if (index < 0 || index >= _bounds.Count) return (Vector3.zero, Vector3.zero);
            return _bounds[index];
        }

        public void Recalculate()
        {
            _atomCount = 0;
            var overallLowest = Vector3.positiveInfinity;
            var overallHighest = Vector3.negativeInfinity;
            foreach (var temp in _storageFormulas)
            {
                var (formula, offset) = temp;
                var atomsQueue = new Queue<(Atom, GenericAtomModel)>();
                var atomsVisited = new HashSet<Atom>();
                
                var atomModelDict = new Dictionary<Atom, GenericAtomModel>();
                
                atomsQueue.Enqueue((formula.GetStartAtom(), null));
                
                var lowest = Vector3.positiveInfinity;
                var highest = Vector3.negativeInfinity;

                #region Atom position calculation

                while (atomsQueue.TryDequeue(out var value))
                {
                    var (atom, prevAtomModel) = value;
                    
                    //Note: Check for visited atoms
                    if (atomsVisited.Contains(atom))
                    {
                        continue;
                    }

                    atomsVisited.Add(atom);

                    //Note: Main logic
                    var atomModel = new GenericAtomModel(atom)
                    {
                        ParentAtom = prevAtomModel
                    };

                    if (!_atoms.ContainsKey(atom.GetElement()))
                    {
                        _atoms[atom.GetElement()] = new List<GenericAtomModel>();
                    }

                    _atoms[atom.GetElement()].Add(atomModel);

                    foreach (var bond in formula.GetAtomBonds(atom))
                    {
                        //Note: if destination atom is visited, skip
                        if (atomsVisited.Contains(bond.GetDestinationAtom())) continue;
                        
                        atomsQueue.Enqueue((bond.GetDestinationAtom(), atomModel));
                    }
                    
                    atomModelDict[atom] = atomModel;
                    
                    //Note: skip first atom
                    if (prevAtomModel != null)
                    {
                        //Note: calculate atom position
                        var curARadius = ElementAtomRadius.Radius[atom.GetElement()];
                        var prevARadius = ElementAtomRadius.Radius[prevAtomModel.GetAtom().GetElement()];
                        var distance = prevARadius + curARadius + 1;
                    
                        var dirVec = _calculator.GetCurrentPosition(formula, atom, prevAtomModel);
                    
                        atomModel.Position = prevAtomModel.Position + dirVec * distance;
                        //Note: add offset
                        atomModel.Position += offset;
                    }
                    
                    if (atomModel.Position.x < lowest.x) lowest.x = atomModel.Position.x;
                    if (atomModel.Position.y < lowest.y) lowest.y = atomModel.Position.y;
                    if (atomModel.Position.z < lowest.z) lowest.z = atomModel.Position.z;
                    
                    if (atomModel.Position.x > highest.x) highest.x = atomModel.Position.x;
                    if (atomModel.Position.y > highest.y) highest.y = atomModel.Position.y;
                    if (atomModel.Position.z > highest.z) highest.z = atomModel.Position.z;
                }
                
                _bounds.Add((lowest, highest));

                #endregion

                #region Overall bounds
                
                if (lowest.x < overallLowest.x) overallLowest.x = lowest.x;
                if (lowest.y < overallLowest.y) overallLowest.y = lowest.y;
                if (lowest.z < overallLowest.z) overallLowest.z = lowest.z;
                
                if (highest.x > overallHighest.x) overallHighest.x = highest.x;
                if (highest.y > overallHighest.y) overallHighest.y = highest.y;
                if (highest.z > overallHighest.z) overallHighest.z = highest.z;

                #endregion

                var structure = formula.CloneStructure();

                //Note: Generate bonds
                foreach( var (atom, model) in atomModelDict)
                {
                    foreach (var bond in structure[atom])
                    {
                        if (!atomModelDict.ContainsKey(bond.GetDestinationAtom())) 
                            throw new Exception("Missing atom model: " + bond.GetDestinationAtom());
                        
                        var destModel = atomModelDict[bond.GetDestinationAtom()];
                
                        var dirVec = destModel.Position - model.Position;
                        var rotation = Quaternion.FromToRotation(Vector3.up, dirVec);
                        var length = dirVec.magnitude;
                        
                        switch (bond.GetBondType())
                        {
                            case Bond.BondType.Single:
                                _1Bonds.Add(new SingleBondModel(model.Position, rotation, length));
                                break;
                            case Bond.BondType.Double:
                                _2Bonds.Add(new DoubleBondModel(model.Position, rotation, length));
                                break;
                            case Bond.BondType.Triple:
                                _3Bonds.Add(new TripleBondModel(model.Position, rotation, length));
                                break;
                            case Bond.BondType.Aromatic:
                                _2Bonds.Add(new DoubleBondModel(model.Position, rotation, length));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        //Note: remove reverse bond from destination atom
                        structure[bond.GetDestinationAtom()].RemoveAll(b => b.GetDestinationAtom() == atom);
                    }
                }
                
                //Note: atom count
                _atomCount += atomModelDict.Count;
            }
        }

        public int GetAtomCount()
        {
            return _atomCount;
        }
    }
}