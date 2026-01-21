using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.game
{
    public static class InteractableManager
    {
        private static readonly Dictionary<GameObject, IInteractable> Interactable = new();
        
        public static void RegisterInteractable(GameObject gameObject, IInteractable interactable)
        {
            Interactable.Add(gameObject, interactable);
        }
        
        public static void UnregisterInteractable(GameObject gameObject)
        {
            Interactable.Remove(gameObject);
        }
        
        public static bool TryGetInteractable(GameObject gameObject, out IInteractable interactable)
        {
            return Interactable.TryGetValue(gameObject, out interactable);
        }
    }
}