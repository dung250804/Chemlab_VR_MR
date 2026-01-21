using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.engine.api.mixture;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.game
{
    public class MixtureContainerManager
    {
        private static readonly Dictionary<GameObject, IMixtureContainer> MixtureContainers = new();
        
        public static void RegisterMixtureContainer(GameObject gameObject, IMixtureContainer mixtureContainer)
        {
            if (MixtureContainers.ContainsKey(gameObject))
            {
                Debug.LogWarning($"MixtureContainerManager: {gameObject} already registered");
                MixtureContainers[gameObject] = mixtureContainer;
                return;
            }
            MixtureContainers.Add(gameObject, mixtureContainer);
        }
        
        public static void UnregisterMixtureContainer(GameObject gameObject)
        {
            MixtureContainers.Remove(gameObject);
        }
        
        public static bool TryGetMixtureContainer(GameObject gameObject, out IMixtureContainer mixtureContainer)
        {
            return MixtureContainers.TryGetValue(gameObject, out mixtureContainer);
        }
    }
}