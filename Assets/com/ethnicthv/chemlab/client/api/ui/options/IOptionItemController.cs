using System;
using com.ethnicthv.util.pool;

namespace com.ethnicthv.chemlab.client.api.ui.options
{
    public interface IOptionItemController : IPoolable
    {
        public int GetHeight();
        public void Setup(string option, Action action);
    }
}