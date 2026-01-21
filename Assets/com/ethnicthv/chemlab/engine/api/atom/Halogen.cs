using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.api.atom
{
    public class Halogen : Atom, IGeneric
    {
        public Halogen() : base(Element.Chlorine)
        {
        }
        
        public bool IsElement(Element element)
        {
            switch (element)
            {
                case Element.Fluorine:
                case Element.Chlorine:
                case Element.Bromine:
                case Element.Iodine:
                    return true;
                default:
                    return false;
            }
        }
    }
}