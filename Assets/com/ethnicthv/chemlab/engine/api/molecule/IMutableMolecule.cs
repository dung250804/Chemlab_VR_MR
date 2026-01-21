using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.molecule.group;

namespace com.ethnicthv.chemlab.engine.api.molecule
{
    public interface IMutableMolecule : IMolecule
    {
        public void AddGroup(MoleculeGroup group);
        public void DeleteGroup(MoleculeGroup group);
        public void AddFunctionalGroup(MoleculeGroup group, IFunctionalGroup atom);
        public void AddFunctionalGroup(MoleculeGroup group, IFunctionalGroup[] atom);
        public void RemoveFunctionalGroup(MoleculeGroup group, IFunctionalGroup atom);
    }
}