namespace com.ethnicthv.chemlab.engine.api.atom
{
    public interface IIsotopeAtom : IAtom
    {
        public int GetNeutronCount();
        public int GetExtraNeutronCount();
    }
}