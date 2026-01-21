using com.ethnicthv.chemlab.engine.api.atom;

namespace com.ethnicthv.chemlab.engine.api.molecule.formula
{
    public interface IBondBreaker
    {
        public void BreakBond(Atom atom1, Atom atom2);
    }
}