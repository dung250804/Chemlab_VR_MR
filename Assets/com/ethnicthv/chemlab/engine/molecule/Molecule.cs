using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.error.molecule;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.api.molecule.formula;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.molecule.group;
using com.ethnicthv.chemlab.engine.reaction;
using Unity.Serialization.Json;
using Unity.VisualScripting;
using UnityEngine;
using Formula = com.ethnicthv.chemlab.engine.formula.Formula;

namespace com.ethnicthv.chemlab.engine.molecule
{
    public class Molecule : IMutableMolecule, IJsonAdapter<Molecule>
    {
        private static readonly Dictionary<string, Molecule> MoleculeRegistry = new();
        
        public static bool IsMolecule(string id)
        {
            return !string.IsNullOrEmpty(id) && MoleculeRegistry.ContainsKey(id);
        }
        
        public static Molecule GetMolecule(string id) {
            if (string.IsNullOrEmpty(id)) return null;

            var molecule = MoleculeRegistry[id];
            if (molecule != null) return molecule;
            try
            {
                return Builder.Create(true)
                    .Structure(Formula.Deserialize(id))
                    .Build();
            }catch (Exception e)
            {
                Debug.LogWarning("Could not find Molecule '"+id+"'. With error: "+e.Message);
            }
            if (!"NO_MOLECULE".Equals(id)) Debug.LogWarning("Could not find Molecule '"+id+"'."); // The 'NO_MOLECULE' is just to stop false warnings due to the Chemical Poison mob effect
            return null;
        }
        
        public static IReadOnlyList<Molecule> GetAllMolecules()
        {
            return MoleculeRegistry.Values.ToList();
        }
        
        private Formula _formula;
        
        private readonly List<ReactingReaction> _reactantReactions = new();
        private readonly List<ReactingReaction> _productReactions = new();

        private readonly Dictionary<MoleculeGroup, List<IFunctionalGroup>> _groups = new();
        private readonly HashSet<MoleculeTag> _tags = new();

        private string _translationKey;

        private string _id;
        private int _charge;
        private float _mass;
        private float _density;
        private float _boilingPoint;
        private float _dipoleMoment;
        private float _molarHeatCapacity;
        private float _latentHeat;
        private int _color;
        private Color _burnColor = Color.red;
        private float _burnIntensity = 1F;
        private Color _gasColor = Color.white;
        private Color _solidColor = Color.white;

        private bool _novel;

        public void Serialize(in JsonSerializationContext<Molecule> context, Molecule value)
        {
            context.Writer.WriteBeginObject();
            context.Writer.WriteKeyValue("id", value._id);
            context.Writer.WriteKeyValue("translationKey", value.GetTranslationKey(false));
            context.Writer.WriteKeyValue("formula", value.GetFrownsCode());
            context.Writer.WriteKey("tags");
            context.Writer.WriteBeginArray();
            foreach (var tag in value._tags)
            {
                context.Writer.WriteValue(tag.ToString());
            }
            context.Writer.WriteEndArray();
            context.Writer.WriteKeyValue("charge", value.GetCharge());
            context.Writer.WriteKeyValue("mass", value.GetMass());
            context.Writer.WriteKeyValue("density", value.GetDensity());
            context.Writer.WriteKeyValue("boilingPoint", value.GetBoilingPoint());
            context.Writer.WriteKeyValue("dipoleMoment", value.GetDipoleMoment());
            context.Writer.WriteKeyValue("molarHeatCapacity", value.GetMolarHeatCapacity());
            context.Writer.WriteKeyValue("latentHeat", value.GetLatentHeat());
            context.Writer.WriteKeyValue("color", value.GetColor());
            context.Writer.WriteKey("burnColor");
            context.Writer.WriteValue(value._burnColor.ToHexString());
            context.Writer.WriteKey("burnIntensity");
            context.Writer.WriteValue(value._burnIntensity);
            context.Writer.WriteKey("gasColor");
            context.Writer.WriteValue(value._gasColor.ToHexString());
            context.Writer.WriteKey("solidColor");
            context.Writer.WriteValue(value._solidColor.ToHexString());
            context.Writer.WriteEndObject();
        }

        public Molecule Deserialize(in JsonDeserializationContext<Molecule> context)
        {
            throw new NotImplementedException();
        }

        private Molecule()
        {
        }

        public Molecule(Formula formula)
        {
            _formula = formula;
        }

        public string GetTranslationKey(bool iupac)
        {
            if (_novel)
            {
                return "chemical.unknown";
            }

            var key = "chemical." + _translationKey;
            var iupacKey = key + ".iupac";
            if (iupac)
            {
                key = iupacKey;
            }

            return key;
        }
        
        public Molecule GetEquivalent() {
            foreach (var molecule in MoleculeRegistry.Values)
            {
                if (!(Math.Abs(GetMass() - molecule.GetMass()) < 0.001)) continue;  // Initially just check the masses match
                if (_formula.Serialize()
                    .Equals(molecule._formula.Serialize())) { // Check the structures match
                    return molecule;
                }
            }
            return this;
        }
        
        public void AddReactantReaction(ReactingReaction reaction) {
            if (reaction.ContainsReactant(this)) _reactantReactions.Add(reaction);
        }
        
        public void AddProductReaction(ReactingReaction reaction) {
            if (reaction.ContainsProduct(this)) _productReactions.Add(reaction);
        }
        
        public IReadOnlyCollection<ReactingReaction> GetReactantReactions() {
            return _reactantReactions;
        }
        
        public IReadOnlyCollection<ReactingReaction> GetProductReactions() {
            return _productReactions;
        }

        public string GetFullID()
        {
            return _id ?? _formula.Serialize();
        }

        public string GetFrownsCode()
        {
            return _formula.Serialize();
        }

        public IFormula GetFormula()
        {
            return _formula;
        }

        public IReadOnlyCollection<MoleculeGroup> GetGroups()
        {
            return _groups.Keys;
        }

        public void DeleteGroup(MoleculeGroup group)
        {
            _groups.Remove(group);
        }

        public void AddGroup(MoleculeGroup group)
        {
            if (!_groups.ContainsKey(group)) _groups.Add(group, new List<IFunctionalGroup>());
        }

        public void AddFunctionalGroup(MoleculeGroup group, IFunctionalGroup atom)
        {
            if (!_groups.ContainsKey(group)) return;
            _groups[group].Add(atom);
        }

        public void AddFunctionalGroup(MoleculeGroup group, IFunctionalGroup[] atom)
        {
            if (!_groups.ContainsKey(group)) return;
            _groups[group].AddRange(atom);
        }

        public void RemoveFunctionalGroup(MoleculeGroup group, IFunctionalGroup atom)
        {
            if (!_groups.ContainsKey(group)) return;
            _groups[group].Remove(atom);
        }

        public IReadOnlyCollection<IFunctionalGroup> GetAtomsInGroup(MoleculeGroup group)
        {
            return _groups[group];
        }

        public bool IsIon()
        {
            return _charge != 0;
        }

        public bool IsAromatic()
        {
            return _formula.IsAromatic;
        }

        public bool IsCyclic()
        {
            return _formula.IsCyclic;
        }

        public void ClearGroups()
        {
            _groups.Clear();
        }

        public int GetCharge()
        {
            return _charge;
        }

        public float GetMass()
        {
            return _mass;
        }

        public float GetDensity()
        {
            return _density;
        }

        public float GetPureConcentration()
        {
            return GetDensity() / GetMass();
        }

        public float GetBoilingPoint()
        {
            return _boilingPoint;
        }

        public float GetDipoleMoment()
        {
            return _dipoleMoment;
        }

        public float GetMolarHeatCapacity()
        {
            return _molarHeatCapacity;
        }

        public float GetLatentHeat()
        {
            return _latentHeat;
        }

        public Formula ShallowCopyStructure()
        {
            return (Formula)_formula.Clone();
        }

        public IReadOnlyList<Atom> GetAtoms()
        {
            return _formula.GetAtoms();
        }

        public bool IsHypothetical()
        {
            return _tags.Contains(MoleculeTag.Hypothetical);
        }
        
        public bool IsSolid()
        {
            return _tags.Contains(MoleculeTag.Solid);
        }
        
        public bool IsUnsolvableGas()
        {
            return _tags.Contains(MoleculeTag.UnsolvableGas);
        }
        
        public bool IsBurnable()
        {
            return _tags.Contains(MoleculeTag.Burnable);
        }

        public HashSet<MoleculeTag> GetTags()
        {
            return new HashSet<MoleculeTag>(_tags);
        }

        public bool HasTag(MoleculeTag tag)
        {
            return _tags.Contains(tag);
        }

        public Dictionary<Element, int> GetMolecularFormula()
        {
            Dictionary<Element, int> empiricalFormula = new();
            foreach (var atom in _formula.GetAtoms())
            {
                var element = atom.GetElement();
                if (empiricalFormula.ContainsKey(element))
                {
                    var count = empiricalFormula[element];
                    empiricalFormula[element] = count + 1;
                }
                else
                {
                    empiricalFormula.Add(element, 1);
                }
            }

            return empiricalFormula;
        }

        public string GetSerlializedMolecularFormula(bool subscript, bool charge = false)
        {
            var formulaMap = GetMolecularFormula();
            List<Element> elements = new(formulaMap.Keys);
            elements.Sort((e1, e2) => e1.CompareTo(e2));
            var formula = "";

            foreach (var element in elements)
            {
                var count = formulaMap[element];
                var number = count == 1 ? "" : count.ToString();
                if (subscript)
                {
                    number = "<sub>" + number + "</sub>";
                }
                formula += ElementProperty.GetElementProperty(element).Symbol + number;
            }

            if (!charge || _charge == 0) return formula;
            var serializedCharge = GetSerializedCharge(true);
            formula += subscript ? "<sup>" + serializedCharge + "</sup>" : serializedCharge;

            return formula;
        }

        public int GetColor()
        {
            return _color;
        }
        
        public Color GetBurnColor()
        {
            return _burnColor;
        }
        
        public float GetBurnIntensity()
        {
            return _burnIntensity;
        }
        
        public Color GetGasColor()
        {
            return _gasColor;
        }

        public bool IsColorless()
        {
            return _color >> 24 == 0;
        }

        public string GetSerializedCharge(bool alwaysShowNumber)
        {
            var chargeString = "";
            if (_charge == 0)
            {
                return chargeString;
            }

            if (alwaysShowNumber || _charge != 1 && _charge != -1)
            {
                chargeString += Math.Abs(_charge);
            }

            chargeString += (_charge < 0 ? "-" : "+");
            return chargeString;
        }

        public class Builder
        {
            private readonly Molecule _molecule = new()
            {
                _charge = 0,
                _density = 1000.0F
            };

            private bool _hasForcedDensity;
            private bool _hasForcedBoilingPoint;
            private bool _hasForcedDipoleMoment;
            private bool _hasForcedMolarHeatCapacity;
            private bool _hasForcedLatentHeat;
            private string _translationKey;

            private bool _novel;

            public static Builder Create(bool isNovel)
            {
                return new Builder
                {
                    _novel = isNovel
                };
            }

            public Builder ID(string id)
            {
                _molecule._id = id;
                TranslationKey(id);
                return this;
            }

            public Builder Structure(Formula structure)
            {
                try
                {
                    _molecule._formula = structure;
                    return this;
                }
                catch (Exception var3)
                {
                    throw E("Cannot use structure.", var3);
                }
            }

            public Builder Density(float density)
            {
                _molecule._density = density;
                _hasForcedDensity = true;
                return this;
            }

            public Builder BoilingPoint(float boilingPoint)
            {
                return BoilingPointInKelvins(boilingPoint + 273.0F);
            }

            public Builder BoilingPointInKelvins(float boilingPoint)
            {
                _molecule._boilingPoint = boilingPoint;
                _hasForcedBoilingPoint = true;
                return this;
            }

            public Builder DipoleMoment(int dipoleMoment)
            {
                _molecule._dipoleMoment = dipoleMoment;
                _hasForcedDipoleMoment = true;
                return this;
            }

            public Builder SpecificHeatCapacity(float specificHeatCapacity)
            {
                return MolarHeatCapacity(specificHeatCapacity / CalculateMass());
            }

            public Builder MolarHeatCapacity(float molarHeatCapacity)
            {
                if (molarHeatCapacity <= 0.0F)
                {
                    throw E("Molar heat capacity must be greater than 0.");
                }

                _molecule._molarHeatCapacity = molarHeatCapacity;
                _hasForcedMolarHeatCapacity = true;
                return this;
            }

            public Builder LatentHeat(float latentHeat)
            {
                if (latentHeat <= 0.0F)
                {
                    throw E("Latent heat of fusion must be greater than 0.");
                }
                else
                {
                    _molecule._latentHeat = latentHeat;
                    _hasForcedLatentHeat = true;
                    return this;
                }
            }

            public Builder TranslationKey(String translationKey)
            {
                _translationKey = translationKey;
                return this;
            }

            public Builder Color(int color)
            {
                _molecule._color = color;
                return this;
            }
            
            public Builder BurnColor(Color color, float intensity)
            {
                _molecule._burnColor = color;
                _molecule._burnIntensity = intensity;
                return this;
            }
            
            public Builder GasColor(Color color)
            {
                _molecule._gasColor = color;
                return this;
            }
            
            public Builder SolidColor(Color color)
            {
                _molecule._solidColor = color;
                return this;
            }

            public Builder Hypothetical()
            {
                return Tag(MoleculeTag.Hypothetical);
            }
            
            public Builder Solid()
            {
                return Tag(MoleculeTag.Solid);
            }
            
            public Builder UnsolvableGas()
            {
                return Tag(MoleculeTag.UnsolvableGas);
            }
            
            public Builder Burnable()
            {
                return Tag(MoleculeTag.Burnable);
            }

            public Builder Tag(params MoleculeTag[] tags)
            {
                var var3 = tags.Length;

                for (var i = 0; i < var3; ++i)
                {
                    //MoleculeTag.registerMoleculeToTag(_molecule, tag);
                    _molecule._tags.Add(tags[i]);
                }

                return this;
            }

            public Molecule Build()
            {
                _molecule._mass = CalculateMass();
                _molecule._translationKey = _translationKey;
                if (_molecule._formula == null)
                {
                    throw E("Molecule's structure has not been declared.");
                }

                if (_molecule.GetAtoms().Count >= 100)
                {
                    throw E("Molecule has too many Atoms");
                }
                
                if (_novel) {
                    Debug.LogWarning("Molecule " + _molecule._id + " is novel, it will not be registered.");
                    var equivalentMolecule = _molecule.GetEquivalent();
                    if (equivalentMolecule != _molecule) {
                        return equivalentMolecule;
                    }
                }

                var charge = _molecule.GetAtoms().Aggregate(0.0f, (current, atom) => current + atom.FormalCharge);

                _molecule._charge = (int)charge;
                if (_molecule.GetMolecularFormula().ContainsKey(Element.RGroup))
                {
                    Hypothetical();
                }

                if (!_hasForcedDensity && _molecule._charge != 0)
                {
                    _molecule._density = EstimateDensity(_molecule);
                }

                if (!_hasForcedBoilingPoint && _molecule._charge == 0)
                {
                    _molecule._boilingPoint = EstimateBoilingPoint(_molecule);
                }

                if (_molecule._charge != 0)
                {
                    BoilingPointInKelvins(float.MaxValue);
                }

                if (!_hasForcedDipoleMoment)
                {
                    _molecule._dipoleMoment = EstimateDipoleMoment(_molecule);
                }

                if (!_hasForcedMolarHeatCapacity)
                {
                    _molecule._molarHeatCapacity = 100.0F;
                }

                if (!_hasForcedLatentHeat)
                {
                    _molecule._latentHeat = 20000.0F;
                }

                if (_molecule._color == 0)
                {
                    _molecule._color = 553648127;
                }
                
                _molecule._novel = _novel;

                _molecule.RefreshFunctionalGroups();
                
                if (!_novel) {
                    if (_molecule._id == null) {
                        throw E("Molecule's ID has not been declared.");
                    }

                    MoleculeRegistry[_molecule.GetFullID()] = _molecule;
                }

                return _molecule;
            }

            private float CalculateMass()
            {
                var atoms = _molecule.GetAtoms();

                return atoms.Sum(atom => ElementProperty.GetElementProperty(atom.GetElement()).AtomicMass);
            }

            private static float EstimateDensity(Molecule molecule)
            {
                return 1000.0F;
            }

            private static float EstimateBoilingPoint(Molecule molecule)
            {
                var hydrogenBondingGroups = 0;
                var carbonyls = 0;
                var halogens = 0;
                var nitriles = 0;
                foreach (var group in molecule._groups.Keys)
                {
                    if (group is MoleculeGroup.Alcohol or MoleculeGroup.NonTertiaryAmine or MoleculeGroup.CarboxylicAcid
                        or MoleculeGroup.UnsubstitutedAmide)
                    {
                        ++hydrogenBondingGroups;
                    }

                    if (group is MoleculeGroup.Carbonyl or MoleculeGroup.CarboxylicAcid
                        or MoleculeGroup.UnsubstitutedAmide)
                    {
                        ++carbonyls;
                    }

                    if (group == MoleculeGroup.Halide)
                    {
                        ++halogens;
                    }

                    if (group == MoleculeGroup.Nitrile)
                    {
                        ++nitriles;
                    }
                }

                return 2.042599F * molecule.GetMass() + 34.32621F * hydrogenBondingGroups +
                       13.089986F * carbonyls - 44.779274F * halogens + 63.981052F * nitriles +
                       178.17686F;
            }

            private static int EstimateDipoleMoment(Molecule molecule)
            {
                return 0;
            }

            private MoleculeConstructionException E(string message)
            {
                return new MoleculeConstructionException(AddInfoToMessage(message), _molecule);
            }

            private MoleculeConstructionException E(string message, Exception e)
            {
                return new MoleculeConstructionException(AddInfoToMessage(message), _molecule, e);
            }

            private string AddInfoToMessage(string message)
            {
                var id = _molecule._id ?? "Unknown ID";
                return "Problem building Molecule (" + id + "): " + message;
            }
        }

        private void RefreshFunctionalGroups()
        {
            GroupDetectingProgram.Instance.CheckMolecule(this);
        }

        public bool IsNovel()
        {
            return _novel;
        }
    }
}