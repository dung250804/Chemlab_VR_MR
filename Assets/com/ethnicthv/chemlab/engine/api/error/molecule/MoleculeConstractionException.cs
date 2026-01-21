using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.error.molecule
{
    public class MoleculeConstructionException : MoleculeException
    {
        public MoleculeConstructionException(string message, Molecule molecule) : base(message, molecule)
        {
        }
        
        public MoleculeConstructionException(string message, Molecule molecule, System.Exception e) : base(message, molecule, e)
        {
        }
    }
}