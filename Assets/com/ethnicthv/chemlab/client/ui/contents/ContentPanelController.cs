using System;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.api.ui.contents;
using com.ethnicthv.chemlab.engine;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.mixture;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.ethnicthv.chemlab.client.ui.contents
{
    public class ContentPanelController : MonoBehaviour, IContentPanelController, IChemicalTicker
    {
        [SerializeField] private ContentListController contentListController;
        [SerializeField] private TextMeshProUGUI temperatureText;
        [FormerlySerializedAs("volumnText")] [SerializeField] private TextMeshProUGUI volumeText;

        private IMixtureContainer _mixtureContainer;
        
        private void Start()
        {
            gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            ChemicalTickerHandler.AddTicker(this);
        }

        private void OnDisable()
        {
            ChemicalTickerHandler.RemoveTicker(this);
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            gameObject.SetActive(true);
            gameObject.transform.SetAsLastSibling();
        }

        public void SetupMixtureToDisplay(IMixtureContainer mixtureContainer)
        {
            if (mixtureContainer == null) return;
            var mixture = mixtureContainer.GetMixture();
            var volume = mixtureContainer.GetVolume();
            contentListController.Setup(mixtureContainer);
            SetTemperatureText(mixture);
            SetVolumnText(volume);
            
            _mixtureContainer = mixtureContainer;
        }

        private void SetTemperatureText(IMixture mixture)
        {
            if (mixture == null)
            {
                temperatureText.text = "Nhiệt độ: không rõ";
                return;
            }
            var temperature = mixture.GetTemperature();
            var celsiusDegree = temperature - 273.15f;
            var roundedCelsiusDegree = Math.Round(celsiusDegree, 2);
            //Note: round to 2 decimal places
            temperatureText.text = $"Nhiệt độ: {roundedCelsiusDegree}°C"; 
        }

        private void SetVolumnText(float volumn)
        {
            if (volumn <= 0)
            {
                volumeText.text = "Dung tích: rỗng!";
                return;
            }
            volumeText.text = volumn < 1 ? 
                $"Dung tích: {Mathf.Round(volumn * 1000)} mL" : 
                $"Dung tích: {Mathf.Round(volumn)} L";
        }

        public void Tick()
        {
            if (_mixtureContainer == null) return;
            UpdateValues();
        }

        private void UpdateValues()
        {
            SetTemperatureText(_mixtureContainer.GetMixture());
            SetVolumnText(_mixtureContainer.GetVolume());
        }
    }
}