using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.molecule.group;

namespace com.ethnicthv.chemlab.engine.molecule.group.functional
{
    public class CarboxylFunctionGroup : IFunctionalGroup
    {
        public CarboxylFunctionGroup(Atom oxygen, Atom carbon, Atom oxygen2, Atom hydrogen)
        {
            Oxygen = oxygen;
            Carbon = carbon;
            Oxygen2 = oxygen2;
            Hydrogen = hydrogen;
        }

        public Atom Oxygen { get; }
        public Atom Carbon { get; }
        public Atom Oxygen2 { get; }
        public Atom Hydrogen { get; }
    }
}