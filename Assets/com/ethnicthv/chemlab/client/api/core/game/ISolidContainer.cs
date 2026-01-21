using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface ISolidContainer
    {
        public void AddSolidMolecule(Molecule solidMolecule, float moles);
        public void RemoveSolidMolecule(Molecule solidMolecule);
        public bool IsSolidEmpty();
        public void ClearSolid();
        public bool ContainsSolidMolecule(Molecule solidMolecule);
        public float GetSolidMoleculeAmount(Molecule solidMolecule);
        public bool TryGetSolidMoleculeAmount(Molecule solidMolecule, out float amount);
    }
}