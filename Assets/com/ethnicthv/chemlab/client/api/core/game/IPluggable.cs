using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IPluggable
    {
        List<IInteractablePlugin> Plugins { get; }

        void TryAddAllPlugins(GameObject gameObject)
        {
            //get all components in gameObject
            var components = gameObject.GetComponents<IInteractablePlugin>();
            
            //add all components to plugins
            foreach (var component in components)
            {
                AddPlugin(component);
            }
        }

        void AddPlugin(IInteractablePlugin plugin)
        {
            Plugins.Add(plugin);
        }
        
        void RemovePlugin(IInteractablePlugin plugin)
        {
            Plugins.Remove(plugin);
        }
        
        T GetPlugin<T>() where T : IInteractablePlugin
        {
            foreach (var plugin in Plugins.OfType<T>())
            {
                return plugin;
            }
            return default;
        }
        
        bool HasPlugin<T>() where T : IInteractablePlugin
        {
            return Plugins.OfType<T>().Any();
        }
        
        bool TryGetPlugin<T>(out T plugin) where T : IInteractablePlugin
        {
            foreach (var p in Plugins.OfType<T>())
            {
                plugin = p;
                return true;
            }
            plugin = default;
            return false;
        }
        
        void ForEachPlugin(System.Action<IInteractablePlugin> action)
        {
            foreach (var plugin in Plugins)
            {
                action(plugin);
            }
        }
    }
}