using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.alltools
{
    public class AllToolsViewController : MonoBehaviour
    {
        private GameObject _prefab;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Transform toolContainer;

        private void OnDisable()
        {
            nameInput.text = "";
        }

        public void SetupView(string typeName, GameObject prefab)
        {
            nameText.text = typeName;
            _prefab = prefab;
            nameInput.text = "";
        }

        public void CreateTool()
        {
            var tool = Instantiate(_prefab, toolContainer);
            var n = nameInput.text;
            var interactablePlugin = tool.transform.GetChild(0).GetComponents<IHasName>();
            if (interactablePlugin.Length > 0)
            {
                interactablePlugin[0].SetName(n);
            }
        }
    }
}