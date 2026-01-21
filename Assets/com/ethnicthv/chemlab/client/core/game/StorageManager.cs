using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.ethnicthv.chemlab.client.core.game
{
    public class StorageManager : MonoBehaviour
    {
        public static StorageManager Instance { get; private set; }

        public string storagePath;
        public Vector3 storagePosition;
        
        private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _loading = new();
        private readonly Dictionary<string, Sprite> _cache = new();
        private readonly LinkedList<IStorable> _storedItems = new();
        
        public event Action OnStorageChanged;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public bool TryGetStorageTex(string key, out AsyncOperationHandle<Sprite> asyncHandle, out Sprite sprite)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                asyncHandle = default;
                sprite = value;
                return true;
            }
            
            sprite = null;
            if (_loading.TryGetValue(key, out var handle))
            {
                asyncHandle = handle;
                return false;
            }
            
            var tex = Addressables.LoadAssetAsync<Sprite>( storagePath + key);
            _loading[key] = tex;
            asyncHandle = tex;
            tex.Completed += operation =>
            {
                _cache[key] = operation.Result;
                _loading.Remove(key);
            };
            return false;
        }

        public void Store(IStorable item)
        {
            _storedItems.AddFirst(item);
            item.GetMainTransform().position = storagePosition;
            OnStorageChanged?.Invoke();
        }
        
        public void Take(IStorable item)
        {
            _storedItems.Remove(item);
            item.GetMainTransform().position = Vector3.zero;
            OnStorageChanged?.Invoke();
        }
        
        public IEnumerable<IStorable> GetStoredItems()
        {
            return _storedItems;
        }
    }
}