using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.util.pool;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.ui.compound
{
    public interface IElementListItemController : IPoolable
    {
        public void Setup(Element element, Color color);
    }
}