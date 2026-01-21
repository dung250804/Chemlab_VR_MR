using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.util.pool;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.storage
{
    public class StoredItemsDisplayController : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Transform contentContainer;
        
        private Pool<StoredItemController> _storedItemPool;
        private Queue<StoredItemController> _storedItems;
        
        private void Awake()
        {
            _storedItemPool = new Pool<StoredItemController>(Factory);
        }
        
        private void OnEnable()
        {
            StorageManager.Instance.OnStorageChanged += OnStorageChanged;
            Setup();
        }
        
        private void OnDisable()
        {
            StorageManager.Instance.OnStorageChanged -= OnStorageChanged;
            Reset();
        }
        
        private void OnStorageChanged()
        {
            Reset();
            Setup();
        }

        private void Reset()
        {
            foreach (var item in _storedItems)
            {
                _storedItemPool.Return(item);
            }
        }

        private void Setup()
        {
            foreach (var item in StorageManager.Instance.GetStoredItems())
            {
                var storedItem = _storedItemPool.Get();
                storedItem.gameObject.SetActive(true);
                storedItem.Setup(item);
                _storedItems.Enqueue(storedItem);
            }
        }

        private StoredItemController Factory()
        {
            var storedItem = Instantiate(itemPrefab, contentContainer);
            return storedItem.GetComponent<StoredItemController>();
        }
    }
}