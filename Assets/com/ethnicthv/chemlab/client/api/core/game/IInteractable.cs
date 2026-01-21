using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IInteractable
    {
        void OnInteract();

        List<(string name, Action onClick)> GetOptions();

        void OnHover();

        (GameObject panelObject, Action<GameObject> setupFunction) GetHoverPanel();
        
        Transform GetMainTransform();

        void OnDrop(GameObject other);

        List<(string name, Action onClick)> GetDropOptions(GameObject other);
    }
}