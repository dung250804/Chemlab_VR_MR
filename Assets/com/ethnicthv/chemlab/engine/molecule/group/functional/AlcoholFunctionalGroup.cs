using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.molecule.group;

namespace com.ethnicthv.chemlab.engine.molecule.group.functional
{
    public class AlcoholFunctionalGroup : IFunctionalGroup
    {
        public AlcoholFunctionalGroup(Atom oxygen, Atom hydrogen)
        {
            Oxygen = oxygen;
            Hydrogen = hydrogen;
        }

        public Atom Oxygen { get; }
        public Atom Hydrogen { get; }
    }
}