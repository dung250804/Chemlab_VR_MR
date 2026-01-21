using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.core.game;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.game
{
    public class IgnitorBehaviour : MonoBehaviour, IInteractable, IPluggable
    {
        public List<IInteractablePlugin> Plugins { get; } = new();

        private IPluggable _pluggable => this;

        private void Awake()
        {
            _pluggable.TryAddAllPlugins(gameObject);
        }

        private void OnEnable()
        {
            InteractableManager.RegisterInteractable(gameObject, this);
        }
        
        private void OnDisable()
        {
            InteractableManager.UnregisterInteractable(gameObject);
        }

        public void OnInteract()
        {
            _pluggable.ForEachPlugin(p => p.OnInteract());
        }

        public List<(string name, Action onClick)> GetOptions()
        {
            var options = new List<(string name, Action onClick)>();
            
            _pluggable.ForEachPlugin(p => p.OnGetOptions(ref options));
            
            return options;
        }

        public void OnHover()
        {
            _pluggable.ForEachPlugin(p => p.OnHover());
        }

        public (GameObject panelObject, Action<GameObject> setupFunction) GetHoverPanel()
        {
            return (null, null);
        }

        public Transform GetMainTransform()
        {
            return transform.parent;
        }

        public void OnDrop(GameObject other)
        {
            _pluggable.ForEachPlugin(p => p.OnDrop(other));
        }

        public List<(string name, Action onClick)> GetDropOptions(GameObject other)
        {
            if (other.TryGetComponent(typeof(IIgnitable), out var ignitable))
            {
                return new List<(string name, Action onClick)>
                {
                    ("Ignite", () => ((IIgnitable) ignitable).Ignite())
                };
            }
            
            return null;
        }
    }
}