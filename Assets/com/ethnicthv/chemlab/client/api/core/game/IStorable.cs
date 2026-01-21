using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IStorable
    {
        public GameObject gameObject { get; }
        
        public string GetStorageTexKey();
        
        public Transform GetMainTransform();
    }
}