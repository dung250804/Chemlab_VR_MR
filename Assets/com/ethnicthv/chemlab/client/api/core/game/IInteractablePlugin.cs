using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IInteractablePlugin
    {
        void OnInteract()
        {
        }

        void OnGetOptions(ref List<(string name, Action onClick)> options)
        {
        }

        void OnHover()
        {
        }

        void OnGetHoverPanel(ref ( GameObject panelObject,
            Action<GameObject> setupFunction) panel)
        {
        }

        void OnDrop(GameObject other)
        {
        }

        void OnGetDropOptions(GameObject other,
            ref List<(string name, Action onClick)> options)
        {
        }
    }
}