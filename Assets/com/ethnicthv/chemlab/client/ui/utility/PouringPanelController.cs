using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.api.ui;
using com.ethnicthv.chemlab.client.api.ui.utility;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.utility
{
    public class PouringPanelController : MonoBehaviour , IPouringPanelController
    {
        [SerializeField] private TMP_InputField amountInputField;
        
        private IMixtureContainer _original;
        private IMixtureContainer _target;
        
        public void SetupPanel(IMixtureContainer original, IMixtureContainer target)
        {
            _original = original;
            _target = target;
        }
        
        public void OpenPanel()
        {
            //Note: open the Utility Layer
            transform.parent.gameObject.SetActive(true);
            
            //Note: open the pouring panel
            gameObject.SetActive(true);
        }

        public void ClosePanel()
        {
            //Note: close the Utility Layer
            transform.parent.gameObject.SetActive(false);
            
            //Note: close the pouring panel
            gameObject.SetActive(false);
            
            //Note: reset the input field
            amountInputField.text = "";
            
            //Note: reset the original and target containers
            _original = null;
            _target = null;
        }

        public void Pour()
        {
            if (_original == null || _target == null)
            {
                Debug.LogError("Original or target container is null");
                return;
            }
            if (!CheckValidPourAmount(out var amount)) return;
            if (_original.GetVolume() < amount)
            {
                Debug.LogError("Not enough volume to pour");
                return;
            }
                
            if (_target.GetVolume() + amount > _target.GetMaxVolume())
            {
                Debug.LogError("Target container is full");
                return;
            }
                
            if (_target.GetMixture() == null)
            {
                _target.SetMixture(_original.GetMixture());
                _target.SetVolume(amount);
            }
            else
            {
                _target.AddMixture(_original.GetMixture(), amount);
            }
                
            _original.SetVolume(_original.GetVolume() - amount);
                
            ClosePanel();
        }

        private bool CheckValidPourAmount(out float amount)
        {
            amount = 0;
            if (float.TryParse(amountInputField.text, out var parsedAmount))
            {
                if (parsedAmount > 0 && parsedAmount <= _original.GetVolume())
                {
                    amount = parsedAmount;
                    return true;
                }
                Debug.LogError("Invalid pour amount");
                return false;
            }

            Debug.LogError("Invalid input");
            return false;
        }
    }
}