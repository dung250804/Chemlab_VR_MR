using com.ethnicthv.chemlab.client.api.ui.compound;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.element;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.compound
{
    public class ElementListItemController : MonoBehaviour, IElementListItemController, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image elementColor;
        [SerializeField] private TextMeshProUGUI elementName;

        private Element _element;
        
        private static readonly Color HoverColor = new(1f, 1f, 1f, 0.1f);
        private static readonly Color NormalColor = new(1f, 1f, 1f, 0f);
        
        public void ResetInstance()
        {
            gameObject.SetActive(false);
        }

        public void Setup(Element element, Color color)
        {
            _element = element;
            elementColor.color = color;
            elementName.text = ElementProperty.GetElementProperty(element).GetName();
            gameObject.SetActive(true);
            gameObject.transform.SetAsFirstSibling();
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
            UIManager.Instance.ElementPanelManager.OpenNewPanel(_element);
        }
    }
}