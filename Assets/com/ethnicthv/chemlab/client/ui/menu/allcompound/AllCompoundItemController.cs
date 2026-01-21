using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.menu.allcompound
{
    [RequireComponent(typeof(Toggle))]
    public class AllCompoundItemController : MonoBehaviour
    {
        private Molecule _molecule;
        private Toggle _button;
        
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private AllCompoundViewController allCompoundViewController;
        
        private void Awake()
        {
            _button = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _button.onValueChanged.AddListener(OnClick);
            if (_button.isOn && _molecule != null)
                allCompoundViewController.SetupView(_molecule);
        }
        
        private void OnDisable()
        {
            _button.onValueChanged.RemoveListener(OnClick);
        }

        public void SetMolecule(Molecule molecule)
        {
            _molecule = molecule;
            
            nameText.text = Translator.Instance.Translate(_molecule.GetTranslationKey(false));
        }

        private void OnClick(bool value)
        {
            if (!value)
                return;
            
            allCompoundViewController.SetupView(_molecule);
        }
    }
}