using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.core.game;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.game.plugin
{
    public class StorablePlugin : MonoBehaviour, IInteractablePlugin, IStorable
    {
        [SerializeField] private string storageTexKey;
        
        public void OnGetOptions(ref List<(string name, Action onClick)> options)
        {
            options.Add(("Store", () =>
            {
                StorageManager.Instance.Store(this);
            }));
        }

        public string GetStorageTexKey()
        {
            return storageTexKey;
        }

        public Transform GetMainTransform()
        {
            return transform.parent;
        }
    }
}