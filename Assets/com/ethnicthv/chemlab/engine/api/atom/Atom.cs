using System.Linq;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.api.atom
{
    public class Atom : IAtom
    {
        private readonly Element _element;

        public int RGroupNumber;
        public readonly float FormalCharge;

        public Atom(Element element, float formalCharge = 0.0f)
        {
            _element = element;
            FormalCharge = formalCharge;
        }
        
        public ElementProperty GetProperty()
        {
            return ElementProperty.GetElementProperty(_element);
        }
        
        public double GetMaxConnectivity()
        {
            return GetProperty().Valences.Max() ;
        }
        
        public override string ToString()
        {
            return _element.ToString();
        }

        public object Clone()
        {
            return new Atom(_element);
        }

        public Element GetElement()
        {
            return _element;
        }

        public float GetMass()
        {
            return GetProperty().AtomicMass;
        }
        
        public bool IsNeutralHydrogen() {
            return _element == Element.Hydrogen && FormalCharge == 0.0D;
        }
    }
}