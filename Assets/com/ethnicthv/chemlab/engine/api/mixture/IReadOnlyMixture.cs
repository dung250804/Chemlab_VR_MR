using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.mixture
{
    public interface IReadOnlyMixture
    {
        public void SetMoles(Molecule molecule, float moles);
    }
}