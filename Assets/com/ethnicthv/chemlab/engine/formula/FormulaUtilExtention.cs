using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.formula
{
    public static class FormulaUtilExtension
    {
        public static Formula AddCarbonyl(this Formula formula)
        {
            var origin = formula.GetCurrentAtom();
            return formula
                .AddAtom(new Atom(Element.Oxygen), Bond.BondType.Double)
                .MoveToAtom(origin);
        }

        public static Formula AddHydroxyl(this Formula formula)
        {
            var origin = formula.GetCurrentAtom();
            return formula
                .AddAtom(new Atom(Element.Oxygen))
                .AddAtom(new Atom(Element.Hydrogen))
                .MoveToAtom(origin);
        }

        public static Formula AddAmine(this Formula formula)
        {
            var origin = formula.GetCurrentAtom();
            var mainNitrogen = new Atom(Element.Nitrogen);
            return formula.AddAtom(mainNitrogen)
                .AddAtom(new Atom(Element.Hydrogen))
                .MoveToAtom(mainNitrogen)
                .AddAtom(new Atom(Element.Hydrogen))
                .MoveToAtom(origin);
        }

        public static Formula AddCarboxyl(this Formula formula)
        {
            var mainCarbon = new Atom(Element.Carbon);
            return formula
                .AddAtom(mainCarbon)
                .AddAtom(new Atom(Element.Oxygen), Bond.BondType.Double)
                .MoveToAtom(mainCarbon)
                .AddAtom(new Atom(Element.Oxygen));
        }
    }
}