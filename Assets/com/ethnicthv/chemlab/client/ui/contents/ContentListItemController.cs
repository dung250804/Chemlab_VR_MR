using System.Globalization;
using com.ethnicthv.chemlab.client.api.ui.contents;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.engine.api.molecule;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.contents
{
    public class ContentListItemController : MonoBehaviour, IContentListItemController, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI moleculeNameText;
        [SerializeField] private TextMeshProUGUI molesText;
        
        private IMolecule _molecule;
        
        private static readonly Color HoverColor = new(1f, 1f, 1f, 0.1f);
        private static readonly Color NormalColor = new(1f, 1f, 1f, 0f);
        
        public void ResetInstance()
        {
            _molecule = null;
            gameObject.SetActive(false);
        }

        public void Setup(IMolecule molecule, float moles)
        {
            _molecule = molecule;
            gameObject.SetActive(true);
            gameObject.transform.SetAsLastSibling();
            moleculeNameText.text = (molecule.IsSolid() ? "(S) " : "") + Translator.Instance.Translate(molecule.GetTranslationKey(false));
            molesText.text = moles.ToString(CultureInfo.InvariantCulture);
        }

        public int GetHeight()
        {
            var rt = (RectTransform) transform;
            return (int) rt.sizeDelta.y;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            background.DOKill();
            background.DOColor(HoverColor, 0.1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            background.DOKill();
            background.DOColor(NormalColor, 0.1f);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_molecule == null)
            {
                Debug.LogError("Molecule is null");
                return;
            }
            UIManager.Instance.CompoundPanelController.SetDisplayedMolecule(_molecule);
            UIManager.Instance.CompoundPanelController.OpenPanel();
        }
    }
}