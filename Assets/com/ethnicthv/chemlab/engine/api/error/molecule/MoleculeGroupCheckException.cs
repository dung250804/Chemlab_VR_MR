using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.error.molecule
{
    public class MoleculeGroupCheckException : MoleculeException
    {
        public MoleculeGroupCheckException(string message, Molecule molecule) : base(message, molecule)
        {
        }
    }
}