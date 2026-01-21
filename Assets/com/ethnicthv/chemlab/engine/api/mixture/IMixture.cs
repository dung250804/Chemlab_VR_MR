using System.Collections;
using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.mixture
{
    public interface IMixture : IReadOnlyMixture
    {
        public void Tick(out bool shouldUpdateFluidMixture);
        public void RemoveMolecule(Molecule molecule);
        public float GetMoles(Molecule molecule);
        public float AddMoles(Molecule molecule, float moles, out bool isMutating);
        public bool ContainMolecule(Molecule solidMolecule);
        IReadOnlyDictionary<Molecule, float> GetMixtureComposition();
        float GetTemperature();
        IReadOnlyList<Molecule> GetMolecules();
    }
}