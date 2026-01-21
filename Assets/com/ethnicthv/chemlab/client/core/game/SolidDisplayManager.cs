using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.game
{
    public class SolidDisplayManager : MonoBehaviour , ISolidDisplayManager
    {
        public static ISolidDisplayManager Instance { get; private set; }
        
        [SerializeField] private SolidDisplay[] solidDisplays;
        [SerializeField] private SolidDisplay defaultSolidDisplay;
        
        private readonly Dictionary<string, SolidDisplay> _solidDisplayMap = new();
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Setup();
        }

        private void Setup()
        {
            foreach (var solidDisplay in solidDisplays)
            {
                _solidDisplayMap[solidDisplay.key] = solidDisplay;
            }
        }
        
        public SolidDisplay GetSolidDisplay(string key)
        {
            return _solidDisplayMap.TryGetValue(key, out var value) ? value : defaultSolidDisplay;
        }
    }
}