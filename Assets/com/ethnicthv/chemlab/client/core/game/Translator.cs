using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.ethnicthv.chemlab.client.core.game
{
    public class Translator : MonoBehaviour , ITranslator
    {
        [Serializable]
        public class Translation
        {
            public string key;
            public string value;
        }
        
        public static ITranslator Instance { get; private set; }
        
        [SerializeField] private List<Translation> translations;
        
        private readonly Dictionary<string, string> _translationMap = new();
        
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
            foreach (var translation in translations)
            {
                _translationMap[translation.key] = translation.value;
            }
        }
        
        public string Translate(string key)
        {
            //Debug.Log("Translating: " + key);
            return _translationMap.TryGetValue(key, out var value) ? value : key;
        }
        
        public bool HasTranslation(string key)
        {
            return _translationMap.ContainsKey(key);
        }
        
        public void AddTranslation(string key, string value)
        {
            _translationMap[key] = value;
        }
    }
}