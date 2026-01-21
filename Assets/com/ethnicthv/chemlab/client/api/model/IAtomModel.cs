using com.ethnicthv.chemlab.engine;
using com.ethnicthv.chemlab.engine.api.atom;

namespace com.ethnicthv.chemlab.client.api.model
{
    public interface IAtomModel : IModel
    {
        public Atom GetAtom();
    }
}