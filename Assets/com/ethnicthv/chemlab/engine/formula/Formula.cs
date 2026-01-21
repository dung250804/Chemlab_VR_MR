using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.error;
using com.ethnicthv.chemlab.engine.api.error.formula;
using com.ethnicthv.chemlab.engine.api.molecule.formula;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine.formula
{
    public class Formula : IFormula
    {
        private Dictionary<Atom, List<Bond>> _structure;

        // <-- mutate by adding Atom to the structure -->
        private Atom _startAtom;
        // <-- end -->

        // <-- mutate by adding structure to the formula -->
        private FormulaRing _rings;
        // <-- end -->

        public bool IsAromatic => _rings.IsAromatic;
        public bool IsCyclic => _rings != null;

        private Atom _currentAtom;
        private string _optimumFrownsCode;

        private Formula()
        {
            _startAtom = null;
            _structure = new Dictionary<Atom, List<Bond>>();
            _rings = null;
        }

        private Formula(Atom startAtom)
        {
            _startAtom = startAtom;
            _structure = new Dictionary<Atom, List<Bond>>();
            _rings = null;
            _currentAtom = startAtom;
            _structure.Add(startAtom, new List<Bond>());
        }

        private Formula(Dictionary<Atom, List<Bond>> structure, Atom startAtom)
        {
            _structure = structure;
            _startAtom = startAtom;
        }

        public object Clone()
        {
            var formula = new Formula
            {
                _structure = CloneStructure()
            };

            formula._rings = _rings.CloneRing(formula);
            formula._startAtom = _startAtom;
            formula._optimumFrownsCode = null;

            return formula;
        }

        #region Builder

        public Atom GetCurrentAtom()
        {
            return _currentAtom;
        }

        public static Formula CreateInstance(Dictionary<Atom, List<Bond>> structure, Atom startAtom)
        {
            return new Formula(structure, startAtom);
        }

        public static Formula CreateNewFormula(Atom startAtom)
        {
            var formula = new Formula
            {
                _startAtom = startAtom,
                _currentAtom = startAtom
            };
            formula._structure.Add(startAtom, new List<Bond>());
            return formula;
        }

        public static Formula CreateNewCarbonFormula()
        {
            return CreateNewFormula(new Atom(Element.Carbon));
        }

        public static Formula CreateNewChainCarbonFormula(int length)
        {
            var formula = CreateNewFormula(new Atom(Element.Carbon));
            for (var i = 0; i < length - 1; i++)
            {
                formula.AddAtom(new Atom(Element.Carbon));
            }

            return formula;
        }

        public static FormulaRing CreateNewRingCarbonFormula(int size)
        {
            if (size < 3)
            {
                throw new Exception("Ring size must be greater than 2");
            }

            var formula = CreateNewFormula(new Atom(Element.Carbon));
            var ring = new FormulaRing(size, formula);
            formula._rings = ring;
            return ring;
        }

        public Formula AddAtom(Atom newAtom, Bond.BondType bondType = Bond.BondType.Single, bool move = true)
        {
            var currentAtomData = CheckAtomData(_currentAtom);
            if (currentAtomData.InRing)
            {
                throw new Exception("Cannot modify Atoms in cycle, use GetRings().AddBranch() instead");
            }
            
            return AddAtomI(newAtom, bondType, move);
        }

        public Formula AddStructure(Formula structure, Bond.BondType bondType = Bond.BondType.Single)
        {
            if (_rings != null && structure.IsCyclic)
                throw new Exception("Cannot add cyclic structure to cyclic formula");

            AddAtom(structure._startAtom, bondType);
            foreach (var (key, value) in structure.GetStructure())
            {
                foreach (var bond in value)
                {
                    AddAtom(key, bond.GetBondType());
                }
            }

            return this;
        }

        private Formula AddGroup(Formula group, bool isSideGroup = true,
            Bond.BondType bondType = Bond.BondType.Single)
        {
            var currentAtomData = CheckAtomData(_currentAtom);
            if (currentAtomData.InRing)
            {
                throw new Exception("Cannot modify Atoms in cycle, use GetRings().AddBranch() instead");
            }
            
            AddGroupToStructure(_structure, _currentAtom, group, bondType);
            if (!isSideGroup)
            {
                _currentAtom = group._currentAtom;
            }

            return this;
        }

        private Formula AddGroupToPosition(Formula group, int position, Bond.BondType bondType = Bond.BondType.Single)
        {
            AddGroupToStructure(_structure, _rings.RingAtoms[position], group, bondType);
            _rings.Branches.Add((position, group._startAtom));
            _currentAtom = group._currentAtom;
            return this;
        }
        
        private Formula AddAtomI(Atom newAtom, Bond.BondType bondType = Bond.BondType.Single, bool move = true)
        {
            //Note: Check current state of the current top atom
            var currentAtomData = CheckAtomData(_currentAtom);
            if (currentAtomData.AvailableConnectivity <= 0)
            {
                throw new Exception("Atom has no available connectivity");
            }

            FormulaHelper.AddAtomToStructure(_currentAtom, newAtom, _structure, bondType);
            if (move)
            {
                _currentAtom = newAtom;
            }

            return this;
        }
        
        public Formula AddAllHydrogens()
        {
            Dictionary<Atom, List<Bond>> newStructure = new(_structure);

            foreach (var atom in _structure.Keys)
            {
                double totalBonds = FormulaHelper.GetTotalConnections(atom, _structure[atom]);
                var t = ElementProperty.GetElementProperty(atom.GetElement()).GetNextLowestValency(totalBonds) -
                        totalBonds;
                if (!(t > 0.0D)) continue;

                var atomData = CheckAtomData(atom);

                if (atomData.InRing)
                {
                    var position = _rings.RingAtoms.IndexOf(atom);
                    for (var j = 0; j < t; j++)
                    {
                        _rings.AddBranch(position, new Atom(Element.Hydrogen));
                    }
                }
                else
                {
                    for (var j = 0; j < t; j++)
                    {
                        FormulaHelper.AddAtomToStructure(atom, new Atom(Element.Hydrogen), newStructure,
                            Bond.BondType.Single);
                    }
                }
            }

            _structure = newStructure;
            return this;
        }
        
        public Formula MoveToAtom(Atom atom)
        {
            _currentAtom = atom;
            return this;
        }

        #endregion

        #region Formula Implementation

        public Dictionary<Atom, List<Bond>> CloneStructure()
        {
            return FormulaHelper.ShallowCopyStructure(_structure);
        }

        public IReadOnlyDictionary<Atom, IReadOnlyList<Bond>> GetStructure()
        {
            var result = new Dictionary<Atom, IReadOnlyList<Bond>>();
            foreach (var (key, value) in _structure)
            {
                result[key] = new ReadOnlyCollection<Bond>(value);
            }

            return new ReadOnlyDictionary<Atom, IReadOnlyList<Bond>>(result);
        }

        public IFormulaRing GetRings()
        {
            return _rings;
        }

        public Atom GetStartAtom()
        {
            return _startAtom;
        }

        public IReadOnlyList<Atom> GetAtoms()
        {
            return _structure.Keys.ToList();
        }

        public IReadOnlyList<Bond> GetBonds()
        {
            return _structure.Values.SelectMany(bonds => bonds).ToList();
        }

        public IReadOnlyList<Bond> GetAtomBonds(Atom atom)
        {
            return _structure[atom];
        }

        public FormulaAtomData CheckAtomData(Atom atom)
        {
            var element = atom.GetElement();
            var isCarbon = element == Element.Carbon;
            var hydrogenCount = isCarbon ? 4 - _structure[atom].Count : 0;
            var availableConnectivity = float.MaxValue;
            var neighbors = new List<Atom>();
            var isInFormula = _structure.ContainsKey(atom);

            if (!isInFormula)
                return new FormulaAtomData(element, false, false, isCarbon, hydrogenCount,
                    availableConnectivity, neighbors);

            var inRing = _rings != null && _rings.RingAtoms.Contains(atom);
            availableConnectivity = (float)FormulaHelper.GetAvailableConnections(atom, _structure[atom]);
            neighbors = _structure[atom].Select(bond => bond.GetDestinationAtom()).ToList();

            return new FormulaAtomData(element, true, inRing, isCarbon, hydrogenCount,
                availableConnectivity, neighbors);
        }

        public void RemoveBond(Atom atom1, Atom atom2)
        {
            var atom1Data = CheckAtomData(atom1);
            var atom2Data = CheckAtomData(atom2);
            switch (atom1Data.InRing)
            {
                case false when !atom2Data.InRing:
                {
                    var bond = _structure[atom1].First(b => b.GetDestinationAtom() == atom2);
                    RemoveBondNonCheckRing(bond);

                    var stack = new Stack<Atom>();

                    stack.Push(bond.GetDestinationAtom());

                    while (stack.TryPop(out var temp))
                    {
                        foreach (var neighbor in _structure[temp]
                                     .Where(neighbor => neighbor.GetDestinationAtom() == atom1))
                        {
                            RemoveBondNonCheckRing(neighbor);
                            stack.Push(neighbor.GetSourceAtom());
                        }
                    }

                    break;
                }
                case true when atom2Data.InRing:
                {
                    _rings.RemoveBond(atom1, atom2);

                    break;
                }
            }
        }

        #endregion

        private void RemoveBondNonCheckRing(Bond bond)
        {
            var sourceAtom = bond.GetSourceAtom();
            var destinationAtom = bond.GetDestinationAtom();
            _structure[sourceAtom].Remove(bond);
            _structure[destinationAtom].Remove(bond);
            if (_structure[sourceAtom].Count == 0)
            {
                _structure.Remove(sourceAtom);
            }

            if (_structure[destinationAtom].Count == 0)
            {
                _structure.Remove(destinationAtom);
            }
        }

        public string Serialize()
        {
            if (_optimumFrownsCode != null)
            {
                return _optimumFrownsCode;
            }

            var body = "";
            var prefix = IsCyclic ? _rings.Topology : "linear";
            if (!IsCyclic)
            {
                var newStructure = FormulaHelper.StripHydrogens(_structure);
                body = FormulaHelper.GetMaximumBranchWithHighestMass(newStructure).Serialize();
            }
            else
            {
                // //updateSideChainStructures();
                // List<Branch> identity = new (topology.getConnections());
                // if (topology.getConnections() > 0)
                // {
                //     for (int i = 0; i < topology.getConnections(); ++i)
                //     {
                //         Formula sideChain =
                //             (Formula)((Pair)sideChains[i]).getSecond();
                //         if (sideChain.getAllAtoms().size() != 0 && !sideChain.startingAtom.isNeutralHydrogen())
                //         {
                //             identity.add(sideChain.getStrippedBranchStartingWithAtom(sideChain.startingAtom));
                //         }
                //         else
                //         {
                //             identity.add(new Branch(new Node(new Atom(Element.HYDROGEN))));
                //         }
                //     }
                // }
                //
                // List<List<Branch>> possibleReflections = new ArrayList(topology.getReflections().length + 1);
                // possibleReflections.add(identity);
                // int[][] var16 = topology.getReflections();
                // int i = var16.length;
                //
                // for (int var7 = 0; var7 < i; ++var7)
                // {
                //     int[] reflectionOrder = var16[var7];
                //     List<Branch> reflection = new ArrayList(topology.getConnections());
                //     int[] var10 = reflectionOrder;
                //     int var11 = reflectionOrder.length;
                //
                //     for (int var12 = 0; var12 < var11; ++var12)
                //     {
                //         int reflectedBranchPosition = var10[var12];
                //         reflection.add((Branch)identity.get(reflectedBranchPosition));
                //     }
                //
                //     possibleReflections.add(reflection);
                // }
                //
                // Collections.sort(possibleReflections, (r1, r2)-> {
                //     return getReflectionComparison(r1).compareTo(getReflectionComparison(r2));
                // });
                // List<Branch> bestReflection = (List)possibleReflections.get(0);
                // if (bestReflection.size() > 0)
                // {
                //     for (i = 0; i < topology.getConnections(); ++i)
                //     {
                //         Branch branch = (Branch)bestReflection[i];
                //         if (!branch.getStartNode().getAtom().isNeutralHydrogen())
                //         {
                //             body = body + branch.serialize();
                //         }
                //
                //         body = body + ",";
                //     }
                // }
                //
                // if (body.length() > 0)
                // {
                //     body = body.substring(0, body.length() - 1);
                // }
            }

            _optimumFrownsCode = prefix + ":" + body;
            return _optimumFrownsCode;
        }

        public class FormulaRing : IFormulaRing
        {
            public string Topology { get; private set; }
            public int MaxConnections { get; private set; }
            public readonly int Size;
            public Atom StartAtom => RingAtoms[0];
            public bool IsAromatic { get; private set; }
            
            private readonly Formula _formula;

            public bool IsUnstable => _numOfDoubleBond is 3 or 4 or > 8;

            internal readonly List<Atom> RingAtoms;
            internal readonly List<(int, Atom)> Branches;

            private int _numOfDoubleBond;
            private bool _isFormed;
            
            public FormulaRing CloneRing(Formula formula)
            {
                var ring = new FormulaRing(Size, formula);
                ring.RingAtoms.AddRange(RingAtoms);
                ring.Branches.AddRange(Branches);
                ring._numOfDoubleBond = _numOfDoubleBond;
                ring._isFormed = _isFormed;
                ring.Topology = Topology;
                ring.MaxConnections = MaxConnections;
                ring.IsAromatic = IsAromatic;
                return ring;
            }

            public FormulaRing(int size, Formula formula)
            {
                if (size < 3)
                {
                    throw new Exception("Ring size must be greater than 2");
                }

                Size = size;
                _formula = formula;
                RingAtoms = new List<Atom>(size);
                Branches = new List<(int, Atom)>();
            }

            public FormulaRing SetAtom(Atom atom, Bond.BondType bondType = Bond.BondType.Single)
            {
                if (bondType == Bond.BondType.Triple) throw new Exception("Cannot set triple bond for ring atom");

                _formula.AddAtomI(atom, bondType);
                RingAtoms.Add(atom);

                if (bondType == Bond.BondType.Double)
                {
                    _numOfDoubleBond++;
                }

                return this;
            }

            private FormulaRing AddBranchC(int position, Atom sideBranch, Bond.BondType bondType = Bond.BondType.Single)
            {
                if (position < 0 || position >= Size)
                {
                    throw new Exception("Branch position is out of range");
                }

                // Note: add sideBranch to formula
                _formula.MoveToAtom(RingAtoms[position]);
                _formula.AddAtomI(sideBranch, bondType);

                Branches.Add((position, sideBranch));

                return this;
            }

            private FormulaRing AddBranchC(int position, Formula sideBranch,
                Bond.BondType bondType = Bond.BondType.Single)
            {
                if (position < 0 || position >= Size)
                {
                    throw new Exception("Branch position is out of range");
                }

                // Note: add sideBranch to formula
                _formula.MoveToAtom(RingAtoms[position]);
                _formula.AddStructure(sideBranch, bondType);

                Branches.Add((position, sideBranch._startAtom));

                return this;
            }

            /// <summary>
            /// A method to form the ring.
            /// This method will create a ring for the formula.
            /// </summary>
            /// <param name="nextCurrentAtomIndex">
            /// The index of the next atom for the builder to continue building the formula.
            /// </param>
            /// <param name="bondType">
            /// The bond type between the start atom and the end atom.
            /// </param>
            /// <returns>
            /// The formula with the formed ring.
            /// </returns>
            /// <exception cref="Exception">
            /// Throw an exception if the ring is already formed or the ring size is not match with the ring structure.
            /// </exception>
            public Formula FormRing(int nextCurrentAtomIndex, string topology,
                Bond.BondType bondType = Bond.BondType.Single)
            {
                if (_isFormed)
                {
                    throw new Exception("Ring is already formed");
                }

                if (Size != RingAtoms.Count)
                {
                    throw new Exception("Ring size is not match with ring structure");
                }

                // Note: set bond between start atom and end atom
                FormulaHelper.AddBondToStructure(StartAtom, _formula._currentAtom, _formula._structure, bondType);
                if (bondType == Bond.BondType.Double)
                {
                    _numOfDoubleBond++;
                }

                // Note: check if ring is aromatic
                if (RingAtoms.Count == 6 && _numOfDoubleBond == 3)
                {
                    var tempBool = 0;
                    for (var i = 0; i < RingAtoms.Count; i++)
                    {
                        if (RingAtoms[i].GetElement() != Element.Carbon)
                        {
                            IsAromatic = false;
                            break;
                        }

                        var bond = _formula.GetAtomBonds(RingAtoms[i])
                            .First(b => b.GetDestinationAtom() == RingAtoms[(i + 1) % 6]);

                        switch (bond.GetBondType())
                        {
                            case Bond.BondType.Single:
                                if (tempBool == 1)
                                {
                                    IsAromatic = false;
                                    break;
                                }

                                tempBool = -1;
                                break;
                            case Bond.BondType.Double:
                                if (tempBool == 1)
                                {
                                    IsAromatic = false;
                                    break;
                                }

                                tempBool = 1;
                                break;
                            case Bond.BondType.Aromatic:
                            case Bond.BondType.Triple:
                            default:
                                IsAromatic = false;
                                break;
                        }

                        if (IsAromatic == false)
                        {
                            break;
                        }
                    }
                }

                var c = RingAtoms.Sum(atom => FormulaHelper.GetAvailableConnections(atom, _formula.GetAtomBonds(atom)));

                // Note: update current atom
                _formula._currentAtom = RingAtoms[nextCurrentAtomIndex];
                _isFormed = true;

                Topology = topology;
                MaxConnections = (int)c;

                return _formula;
            }

            public void RemoveBond(Atom atom1, Atom atom2)
            {
                if (!RingAtoms.Contains(atom1) || !RingAtoms.Contains(atom2)) return;

                _formula.RemoveBondNonCheckRing(
                    _formula.GetAtomBonds(atom1).First((b) => b.GetDestinationAtom() == atom2));
                _formula._rings = null;
            }

            public IReadOnlyList<Atom> GetRingAtoms()
            {
                return RingAtoms;
            }

            public IReadOnlyList<(int, Atom)> GetBranches()
            {
                return Branches;
            }

            public void AddBranch(int position, Atom sideBranch,
                Bond.BondType bondType = Bond.BondType.Single)
            {
                AddBranchC(position, sideBranch, bondType);
            }

            public void AddBranch(int position, IFormula sideBranch, Bond.BondType bondType = Bond.BondType.Single)
            {
                if (sideBranch is Formula formula)
                {
                    AddBranchC(position, formula, bondType);
                }
                else
                {
                    throw new Exception("Cannot add non-formula side branch");
                }
            }

            public FormulaAtomData CheckAtomData(Atom atom)
            {
                return _formula.CheckAtomData(atom);
            }

            public int GetConnections()
            {
                return MaxConnections;
            }
        }
        
        public static Formula Deserialize(string FROWNSstring)
        {
            try
            {
                var topologyAndFormula = FROWNSstring.Trim().Split(":");
                if (topologyAndFormula.Length != 2)
                {
                    throw new FormulaDeserializationException("Invalid FROWNS String: '" + FROWNSstring + "'");
                }

                var topology = topologyAndFormula[0];
                var formulaString = topologyAndFormula[1];
                Formula formula;
                if (topology == "linear")
                {
                    var symbols = Regex.Split(formulaString, "(?=\\p{Lu})").ToList();
                    symbols.Remove(symbols[0]);
                    formula = CreateGroupFromString(symbols);
                }
                else
                {
                    var t = FormulaTopology.GetTopology(topology);
                    if (t == null)
                    {
                        throw new FormulaDeserializationException(
                            "Missing base formula for Topology: " + topology);
                    }

                    formula = t.Factory.Invoke();
                    // if (formula._rings.GetConnections() == 0)
                    // {
                    //     return formula.refreshFunctionalGroups();
                    // }

                    var i = 0;
                    var var6 = formulaString.Split(",");
                    var var7 = var6.Length;

                    for (var var8 = 0; var8 < var7; ++var8)
                    {
                        var group = var6[var8];
                        if (i > formula._rings.GetConnections())
                        {
                            throw new FormulaDeserializationException("Formula '" +
                                                                       FROWNSstring +
                                                                       "' has too many groups for its Topology. There should be " +
                                                                       formula._rings.GetConnections() + ".");
                        }

                        var sideChain = string.IsNullOrWhiteSpace(group)
                            ? new Formula(new Atom(Element.Hydrogen))
                            : CreateGroupFromString(Regex.Split(group, "(?=\\p{Lu})").ToList());

                        //TODO: add way to specify the bond type
                        formula.AddGroupToPosition(sideChain, i);
                        ++i;
                    }
                }

                formula.AddAllHydrogens()/*.refreshFunctionalGroups()*/;
                //formula.updateSideChainStructures();

                formula._optimumFrownsCode = FROWNSstring;
                return formula;
            }
            catch (Exception var11)
            {
                throw new("Could not parse FROWNS String '" + FROWNSstring + "'", var11);
            }
        }
        
        private static Formula CreateGroupFromString(IReadOnlyList<string> symbols)
        {
            var formula = new Formula();
            var hasFormulaBeenInstantiated = false;
            
            var nextAtomBond = Bond.BondType.Single;

            var i = 0;
            while (i < symbols.Count)
            {
                if (Regex.IsMatch(symbols[i], ".*\\)"))
                {
                    throw new Exception(
                        "Chain bond type symbols must preceed side groups; for example chloroethene must be 'linear:C=(Cl)C' and not 'linear:C(Cl)=C'.");
                }

                Dictionary<Formula, Bond.BondType> groupsToAdd = new();
                var thisAtomBond = nextAtomBond;
                string symbol;
                
                if (!symbols[i].Contains("("))
                {
                    symbol = symbols[i];
                }
                else
                {
                    var groupBond = FormulaHelper.TrailingBondType(symbols[i]);
                    
                    symbol = symbols[i][..symbols[i].IndexOf('(')];
                    
                    var brackets = 1;
                    var subSymbols = new List<string>();

                    while (brackets > 0)
                    {
                        i++;
                        var added = false;

                        for (var j = 0; j < symbols[i].Length; ++j)
                        {
                            var c = symbols[i][j];
                            switch (c)
                            {
                                case ')':
                                    --brackets;
                                    break;
                                case '(':
                                    ++brackets;
                                    break;
                            }

                            if (brackets != 0) continue;
                            subSymbols.Add(symbols[i][..j]);
                            groupsToAdd.Add(CreateGroupFromString(subSymbols), groupBond);
                            subSymbols = new List<string>();
                            groupBond = FormulaHelper.TrailingBondType(symbols[i]);
                            added = true;
                        }

                        if (!added)
                        {
                            subSymbols.Add(symbols[i]);
                        }
                    }
                }

                var stripBond = true;
                nextAtomBond = Bond.BondType.Single;
                switch (symbol[^1])
                {
                    case '=':
                        nextAtomBond = Bond.BondType.Double;
                        break;
                    case '#':
                        nextAtomBond = Bond.BondType.Triple;
                        break;
                    case '~':
                        nextAtomBond = Bond.BondType.Aromatic;
                        break;
                    default:
                        stripBond = false;
                        break;
                }

                if (stripBond) symbol = symbol[..^1];

                // Check for charge
                var charge = 0.0f;
                var symbolAndCharge = symbol.Split("^");
                
                if (symbolAndCharge.Length != 1)
                {
                    symbol = symbolAndCharge[0];
                    charge = float.Parse(symbolAndCharge[1]);
                }

                // Check if this is a numbered R-Group
                var lastChar = symbol[^1];
                var rGroupNumber = 0;
                if (char.IsDigit(lastChar))
                {
                    symbol = symbol[..^1];
                    rGroupNumber = int.Parse((lastChar - '0').ToString());
                }

                var atom = new Atom(ElementProperty.GetElementProperty(symbol).GetElement(), charge)
                {
                    RGroupNumber = rGroupNumber
                };
                
                // Add the Atom to the Formula
                if (hasFormulaBeenInstantiated)
                { //if this is not the first Atom
                    formula.AddGroup(new Formula(atom), false, thisAtomBond);
                }
                else
                {
                    formula = new Formula(atom);
                    hasFormulaBeenInstantiated = true;
                }

                foreach (var group in groupsToAdd.Keys)
                {
                    formula.AddGroup(group, true, groupsToAdd[group]);
                }

                i++;
            }

            return formula;
        }
        
        private static void AddGroupToStructure(Dictionary<Atom, List<Bond>> structureToMutate, Atom rootAtom,
            Formula group, Bond.BondType bondType)
        {
            if (group.IsCyclic)
            {
                throw new Exception(
                    "Cannot add Cycles as side-groups - to create a Cyclic Molecule, start with the Cycle and use addGroupAtPosition(), or use Formula.joinFormulae if this is in a Generic Reaction");
            }

            foreach (var entry in group._structure)
            {
                if (structureToMutate.ContainsKey(entry.Key))
                {
                    throw new Exception("Cannot add a derivative of a Formula to itself.");
                }

                structureToMutate.Add(entry.Key, entry.Value);
            }

            FormulaHelper.AddBondToStructure(rootAtom, group._startAtom, structureToMutate, bondType);
        }
        
        public static Formula JoinFormulae(Formula formula1, Formula formula2, Bond.BondType bondType) {
            Formula formula;
            if (formula2.IsCyclic) {
                if (formula1.IsCyclic) {
                    throw new FormulaModificationException(formula1, "Cannot join two cyclic structures.");
                }

                formula1._startAtom = formula1._currentAtom;
                formula2.AddGroup(formula1, false, bondType);
                formula = formula2;
            } else {
                formula2._startAtom = formula2._currentAtom;
                formula1.AddGroup(formula2, true, bondType);
                formula = formula1;
            }

            return (Formula)formula.Clone();
        }
    }
}