using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.api.molecule.formula
{
    public record FormulaAtomData(
        Element Element,
        bool IsInFormula,
        bool InRing,
        bool IsCarbon,
        int HydrogenCount,
        float AvailableConnectivity,
        IReadOnlyList<Atom> Neighbors
    );
}