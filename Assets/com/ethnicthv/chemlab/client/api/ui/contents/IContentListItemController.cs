using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.util.pool;

namespace com.ethnicthv.chemlab.client.api.ui.contents
{
    public interface IContentListItemController : IPoolable
    {
        public void Setup(IMolecule molecule, float moles);
        public int GetHeight();
    }
}