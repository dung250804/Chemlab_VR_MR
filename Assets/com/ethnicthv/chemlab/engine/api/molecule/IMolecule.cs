using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.molecule.formula;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.formula;

namespace com.ethnicthv.chemlab.engine.api.molecule
{
    public interface IMolecule
    {
        public IFormula GetFormula();
        public IReadOnlyCollection<MoleculeGroup> GetGroups();
        public IReadOnlyCollection<IFunctionalGroup> GetAtomsInGroup(MoleculeGroup group);
        public bool IsIon();
        public bool IsAromatic();
        public bool IsCyclic();
        public int GetCharge();
        public float GetMass();
        public float GetDensity();
        public float GetPureConcentration();
        public float GetBoilingPoint();
        public float GetDipoleMoment();
        public float GetMolarHeatCapacity();
        public float GetLatentHeat();
        public Formula ShallowCopyStructure();
        public IReadOnlyList<Atom> GetAtoms();
        public bool IsHypothetical();
        public HashSet<MoleculeTag> GetTags();
        public bool HasTag(MoleculeTag tag);
        public Dictionary<Element, int> GetMolecularFormula();
        public string GetSerlializedMolecularFormula(bool subscript, bool charge = false);
        public int GetColor();
        public bool IsColorless();
        public string GetSerializedCharge(bool alwaysShowNumber);
        string GetTranslationKey(bool b);
        bool IsSolid();
    }
}