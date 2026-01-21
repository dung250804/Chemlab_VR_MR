using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.api.ui.utility;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.utility
{
    public class NamingPanelController : MonoBehaviour, INamingPanelController
    {
        [SerializeField] private TMP_InputField nameInputField;
        
        private IHasName _target;

        public void SetupPanel(IHasName target)
        {
            _target = target;
        }

        public void OpenPanel()
        {
            //Note: open the Utility Layer
            transform.parent.gameObject.SetActive(true);
            
            //Note: open the naming panel
            gameObject.SetActive(true);
            
            //Note: set the input field text to the current name
            nameInputField.text = _target.GetName();
        }

        public void ClosePanel()
        {
            //Note: close the Utility Layer
            transform.parent.gameObject.SetActive(false);
            
            //Note: close the naming panel
            gameObject.SetActive(false);
            
            //Note: reset the input field
            nameInputField.text = "";
            
            //Note: reset the target container
            _target = null;
        }

        public void Rename()
        {
            if (_target == null)
            {
                Debug.LogError("Target container is null");
                return;
            }
                
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                Debug.LogError("Invalid name");
                return;
            }
                
            _target.SetName(nameInputField.text);
                
            ClosePanel();
        }
    }
}