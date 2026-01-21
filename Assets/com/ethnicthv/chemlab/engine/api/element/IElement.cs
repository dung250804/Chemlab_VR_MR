using System.Collections.Generic;

namespace com.ethnicthv.chemlab.engine.api.element
{
    public interface IElement
    {
        public float GetAtomicMass();
        public int GetAtomicNumber();
        public string GetSymbol();
        public string GetName();
        public string GetElectronConfiguration();
        public ElementGroup GetGroup();
        public float GetDensity();
        public float GetMeltingPoint();
        public float GetBoilingPoint();
        public float GetElectronegativity();
        public IReadOnlyList<double> GetValences();
    }
}