using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IInstrument
    {
        public GameObject gameObject { get; }
        
        public Transform GetMainTransform();
    }
}