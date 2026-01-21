using System;
using com.ethnicthv.chemlab.client.api.ui.options;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.options
{
    public class OptionItemController : MonoBehaviour, IOptionItemController, 
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI text;
        
        private Action _action;
        
        private static readonly Color HoverColor = new(1f, 1f, 1f, 0.1f);
        private static readonly Color NormalColor = new(1f, 1f, 1f, 0f);
        
        public int GetHeight()
        {
            var rt = (RectTransform) transform;
            
            return (int) rt.rect.height;
        }

        public void Setup(string option, Action action)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            text.text = option;
            _action = action;
        }

        public void ResetInstance()
        {
            _action = null;
            gameObject.SetActive(false);
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
            _action?.Invoke();
        }
    }
}