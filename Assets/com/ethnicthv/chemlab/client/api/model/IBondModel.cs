using com.ethnicthv.chemlab.engine;
using com.ethnicthv.chemlab.engine.api;

namespace com.ethnicthv.chemlab.client.api.model
{
    public interface IBondModel : IModel
    {
        public Bond GetBond();
        public Bond.BondType GetBondType();
    }
}