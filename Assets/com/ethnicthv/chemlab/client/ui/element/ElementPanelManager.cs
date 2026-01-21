using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.ui.element;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.util.pool;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.element
{
    public class ElementPanelManager : MonoBehaviour, IElementPanelManager
    {
        [SerializeField] private GameObject elementPanelPrefab;
        [SerializeField] private RectTransform elementPanelParent;
        
        private readonly Dictionary<Element, ElementPanelController> _elementPanels = new();
        private Pool<ElementPanelController> _elementPanelPool;

        private void Awake()
        {
            _elementPanelPool = new Pool<ElementPanelController>(Factory);
        }

        public void OpenNewPanel(Element element)
        {
            if (_elementPanels.TryGetValue(element, out var panel))
            {
                panel.OpenPanel();
                return;
            }
            var elementPanel = _elementPanelPool.Get();
            elementPanel.SetupPanel(element);
            elementPanel.OpenPanel();
            _elementPanels.Add(element, elementPanel);
        }

        private void OnClose(Element element)
        {
            if (_elementPanels.Remove(element, out var panel))
            {
                _elementPanelPool.Return(panel);
            }
        }

        private ElementPanelController Factory()
        {
            return Instantiate(elementPanelPrefab, elementPanelParent)
                .GetComponent<ElementPanelController>()
                .SetOnClose(OnClose);
        }
    }
}