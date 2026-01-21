using System;
using System.Collections;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.client.core.renderer;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.menu.allcompound
{
    public class AllCompoundViewController : MonoBehaviour
    {
        private Molecule _displayedMolecule;
        
        private Coroutine _centerRenderedMoleculeCoroutine;
        
        [Header("Display")]
        [SerializeField] private Transform cameraBox;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI chargeText;
        
        [Header("Details")]
        [SerializeField] private TextMeshProUGUI formulaText;
        [SerializeField] private TextMeshProUGUI simpleFormulaText;
        [SerializeField] private TextMeshProUGUI massText;
        [SerializeField] private TextMeshProUGUI densityText;
        [SerializeField] private TextMeshProUGUI boilingPointText;
        [SerializeField] private TextMeshProUGUI boilingPointCText;
        [SerializeField] private GameObject isSolidState;
        [SerializeField] private Transform tagContainer;
        [SerializeField] private GameObject tagPrefab;
        
        [Header("Add to Bottle")]
        [SerializeField] private Button addToBottleButton;

        public void SetupView(Molecule molecule)
        {
            //Note: unregister previous molecule
            RenderProgram.Instance.ClearRenderEntity();
            
            //Note: update molecule displayed
            _displayedMolecule = molecule;
            RenderProgram.Instance.RegisterRenderEntity(_displayedMolecule.GetFormula(), Vector3.zero);
            
            if (_centerRenderedMoleculeCoroutine != null)
            {
                StopCoroutine(_centerRenderedMoleculeCoroutine);
            }
            
            _centerRenderedMoleculeCoroutine = StartCoroutine(CenterRenderedMoleculeCoroutine());

            nameText.text = Translator.Instance.Translate(_displayedMolecule.GetTranslationKey(false));
            chargeText.text = _displayedMolecule.GetSerializedCharge(false);
            
            formulaText.text = _displayedMolecule.GetFrownsCode();
            simpleFormulaText.text = _displayedMolecule.GetSerlializedMolecularFormula(true,true);
            massText.text = _displayedMolecule.GetMass().ToString("F2");
            densityText.text = _displayedMolecule.GetDensity().ToString("F2");
            var bl = _displayedMolecule.GetBoilingPoint();
            if (bl >= 10000000000)
            {
                boilingPointText.text = "N/A";
                boilingPointCText.text = "N/A";
            }
            else
            {
                boilingPointText.text = bl.ToString("F2");
                boilingPointCText.text = (bl - 273.15f).ToString("F2");
            }
            isSolidState.SetActive(_displayedMolecule.IsSolid());
            foreach (Transform child in tagContainer)
            {
                Destroy(child.gameObject);
            }
            foreach (var moleculeTag in _displayedMolecule.GetTags())
            {
                var tagObject = Instantiate(tagPrefab, tagContainer);
                var text = tagObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                text.text = moleculeTag.ToString();
                tagObject.SetActive(true);
            }
        }
        
        private IEnumerator CenterRenderedMoleculeCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            
            if (!RenderProgram.Instance.HasAnyRenderEntity()) yield break;
            
            var (lower, higher) = RenderProgram.Instance.GetBound(0);
            
            var center = (lower + higher) / 2;
            
            cameraBox.transform.position = center;
        }

        public void AddToBottle()
        {
            UIManager.Instance.Utility.AddMoleculePanelController.SetupPanel(_displayedMolecule);
            UIManager.Instance.Utility.AddMoleculePanelController.OpenPanel();
        }
    }
}