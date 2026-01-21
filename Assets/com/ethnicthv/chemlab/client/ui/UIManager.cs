using com.ethnicthv.chemlab.client.api.ui;
using com.ethnicthv.chemlab.client.api.ui.compound;
using com.ethnicthv.chemlab.client.api.ui.contents;
using com.ethnicthv.chemlab.client.api.ui.element;
using com.ethnicthv.chemlab.client.api.ui.options;
using com.ethnicthv.chemlab.client.ui.compound;
using com.ethnicthv.chemlab.client.ui.contents;
using com.ethnicthv.chemlab.client.ui.element;
using com.ethnicthv.chemlab.client.ui.options;
using com.ethnicthv.chemlab.client.ui.utility;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        public static IUIManager Instance { get; private set; }
        
        public ICompoundPanelController CompoundPanelController => compoundPanelController;
        public IElementPanelManager ElementPanelManager => elementPanelManager;
        public IOptionsPanelController OptionsPanelController => optionsPanelController;
        public IContentPanelController ContentPanelController => contentPanelController;
        
        public UtilityUIManager Utility => utilityUIManager;
        
        [SerializeField] private UtilityUIManager utilityUIManager;
        [SerializeField] private CompoundPanelController compoundPanelController; 
        [SerializeField] private ElementPanelManager elementPanelManager;
        [SerializeField] private OptionsPanelController optionsPanelController;
        [SerializeField] private ContentPanelController contentPanelController;
        
        [Space(10)]
        [SerializeField] private Transform hoverPanelContainer;
        
        private GameObject _currentHoverPanel;
        
        private void Awake()
        {
            Instance = this;
        }
        
        public bool IsHoverPanelOpen()
        {
            return _currentHoverPanel != null;
        }
        
        public GameObject OpenHoverPanel(GameObject hoverPanelPrefab)
        {
            if (_currentHoverPanel != null)
            {
                CloseHoverPanel();
            }
            
            _currentHoverPanel = Instantiate(hoverPanelPrefab, hoverPanelContainer);
            return _currentHoverPanel;
        }
        
        public void CloseHoverPanel()
        {
            if (_currentHoverPanel == null) return;
            Destroy(_currentHoverPanel);
            _currentHoverPanel = null;
        }
    }
}