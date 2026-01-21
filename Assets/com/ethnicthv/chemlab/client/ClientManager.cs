using UnityEngine;

namespace com.ethnicthv.chemlab.client
{
    public class ClientManager : MonoBehaviour
    {
        public static ClientManager Instance { get; private set; }
        
        public Camera mainCamera;
        
        private void Awake()
        {
            Instance = this;
        }
    }
}