using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.formula
{
    public class FormulaTopologies
    {
        private static Formula BenzeneFactory()
        {
            var f = Formula.CreateNewRingCarbonFormula(6);
            return f
                .SetAtom(new Atom(Element.Carbon), Bond.BondType.Double)
                .SetAtom(new Atom(Element.Carbon), Bond.BondType.Single)
                .SetAtom(new Atom(Element.Carbon), Bond.BondType.Double)
                .SetAtom(new Atom(Element.Carbon), Bond.BondType.Single)
                .SetAtom(new Atom(Element.Carbon), Bond.BondType.Double)
                .FormRing(5, "benzene");
        }
        
        public static FormulaTopology Benzene = new FormulaTopology(BenzeneFactory, "benzene");
    }
}