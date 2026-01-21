using System;
using System.Collections;
using com.ethnicthv.chemlab.client.api.ui.compound;
using com.ethnicthv.chemlab.client.core.renderer;
using com.ethnicthv.chemlab.engine.api.molecule;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.compound
{
    public class CompoundPanelController : MonoBehaviour, ICompoundPanelController
    {
        [SerializeField] private Transform cameraBox;
        [SerializeField] private TextMeshProUGUI compoundName;
        [SerializeField] private ElementListPanelController elementListPanelController;
        
        private IMolecule _molecule2Display;
        
        private Coroutine _centerRenderedMoleculeCoroutine;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            gameObject.SetActive(true);
            gameObject.transform.SetAsLastSibling();
            _centerRenderedMoleculeCoroutine = StartCoroutine(CenterRenderedMoleculeCoroutine());
        }
        
        public void SetDisplayedMolecule(IMolecule molecule)
        {
            //Note: unregister previous molecule
            if (_molecule2Display != null)
            {
                RenderProgram.Instance.UnregisterRenderEntity(_molecule2Display.GetFormula());
            }
            
            //Note: update molecule displayed
            _molecule2Display = molecule;
            compoundName.text = _molecule2Display.GetTranslationKey(false);
            RenderProgram.Instance.RegisterRenderEntity(_molecule2Display.GetFormula(), Vector3.zero);
            
            if (_centerRenderedMoleculeCoroutine != null)
            {
                StopCoroutine(_centerRenderedMoleculeCoroutine);
            }
        }
        
        private IEnumerator CenterRenderedMoleculeCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            
            if (!RenderProgram.Instance.HasAnyRenderEntity()) yield break;
            
            var (lower, higher) = RenderProgram.Instance.GetBound(0);
            
            var center = (lower + higher) / 2;
            
            cameraBox.transform.position = center;
            
            //Note: update element list
            elementListPanelController.SetElementList(RenderProgram.Instance.GetElementColors());
        }
    }
}