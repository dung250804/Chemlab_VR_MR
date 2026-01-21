using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.menu.alltools
{
    [RequireComponent(typeof(Toggle))]
    public class AllToolsItemController : MonoBehaviour
    {
        private GameObject _prefab;
        private string _type;
        private Toggle _button;
        
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private AllToolsViewController allToolsViewController;
        
        private void Awake()
        {
            _button = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _button.onValueChanged.AddListener(OnClick);
            if (_button.isOn && _prefab != null)
                allToolsViewController.SetupView(_type, _prefab);
        }
        
        private void OnDisable()
        {
            _button.onValueChanged.RemoveListener(OnClick);
        }

        public void Set(string type, GameObject prefab)
        {
            _prefab = prefab;
            _type = type;
            
            nameText.text = type;
        }

        private void OnClick(bool value)
        {
            if (!value)
                return;
            
            allToolsViewController.SetupView(_type ,_prefab);
        }
    }
}