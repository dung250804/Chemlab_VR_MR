using UnityEngine;
using UnityEngine.Serialization;

namespace com.ethnicthv.chemlab.client.core.game
{
    public class Environment : MonoBehaviour
    {
        [SerializeField] private float temperature = 298f;
        
        public static Environment Instance { get; private set; }
        
        public float Temperature
        {
            get => temperature;
            set => temperature = value;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}