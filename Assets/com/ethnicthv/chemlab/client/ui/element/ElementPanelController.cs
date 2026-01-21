using System;
using System.Globalization;
using System.Linq;
using com.ethnicthv.chemlab.client.api.ui.element;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.util.pool;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.element
{
    public class ElementPanelController : MonoBehaviour, IElementPanelController, IPoolable
    {
        [SerializeField] private ElementDisplayer elementDisplayer;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private TextMeshProUGUI atomicNumberText;
        [SerializeField] private TextMeshProUGUI electronConfigurationText;
        [SerializeField] private TextMeshProUGUI atomicMassText;
        [SerializeField] private TextMeshProUGUI valenceElectronsText;
        
        private Element _element;
        private ElementProperty _elementProperty;
        private Action<Element> _onClose;
        
        public void OpenPanel()
        {
            gameObject.SetActive(true);
            gameObject.transform.SetAsLastSibling();
        }

        public void ClosePanel()
        {
            _onClose(_element);
            gameObject.SetActive(false);
        }

        public void SetupPanel(Element element)
        {
            _element = element;
            _elementProperty = ElementProperty.GetElementProperty(element);
            
            var electronConfiguration = _elementProperty.ElectronConfiguration;
            var electronDistribution = ElementUtil.AnalyzeElectronConfiguration(electronConfiguration);
            elementDisplayer.SetupDisplay(_elementProperty.GetSymbol(), electronDistribution);
            
            nameText.text = _elementProperty.GetName();
            symbolText.text = _elementProperty.GetSymbol();
            atomicNumberText.text = _elementProperty.GetAtomicNumber().ToString();
            electronConfigurationText.text = electronConfiguration;
            atomicMassText.text = _elementProperty.GetAtomicMass().ToString(CultureInfo.CurrentCulture);
            if (_elementProperty.GetValences().Count == 1 && _elementProperty.GetValences()[0] == 0)
            {
                valenceElectronsText.text = "Không";
                return;
            }
            var text = _elementProperty.GetValences().Aggregate("" , (current, valence) => current + valence + ", ");
            text = text.Remove(text.Length - 2);
            valenceElectronsText.text = text;
        }

        public void ResetInstance()
        {
            gameObject.transform.localPosition = Vector3.zero;
        }
        
        public ElementPanelController SetOnClose(Action<Element> onClose)
        {
            _onClose = onClose;
            return this;
        }
    }
}