using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.atom;

namespace com.ethnicthv.chemlab.engine.api.molecule.group
{
    public record DetectingContext
    {
        public IMolecule Molecule { get; init; }
        public List<Atom> AtomList { get; init; }
    }
}