using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.ui;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.ethnicthv.chemlab.client.game.plugin
{
    public class NamePlugin : MonoBehaviour, IInteractablePlugin, IHasName
    {
        [SerializeField] private string interactableName;
        
        [SerializeField] private TextMeshPro nameText;

        public void OnGetOptions(ref List<(string name, Action onClick)> options)
        {
            options.Add(("Rename", Rename));
        }

        public string GetName()
        {
            return interactableName;
        }

        public void SetName(string newName)
        {
            interactableName = newName;
            nameText.text = newName;
        }

        private void Rename()
        {
            UIManager.Instance.Utility.NamingPanelController.SetupPanel(this);
            UIManager.Instance.Utility.NamingPanelController.OpenPanel();
        }
    }
}