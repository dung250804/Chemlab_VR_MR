using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.client.api.ui.options;
using com.ethnicthv.util.pool;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.options
{
    public class OptionsPanelController : MonoBehaviour, IOptionsPanelController
    {
        [SerializeField] float animationDuration = 0.2f;
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private RectTransform optionsParent;
        
        private Pool<IOptionItemController> _elementListItemPool;
        private readonly Queue<IOptionItemController> _activeElementListItems = new();
        
        private IReadOnlyList<(string, Action)> _options;
        
        private bool _open = false;
        
        private void Awake()
        {
            _elementListItemPool = new Pool<IOptionItemController>(Factory);
            
            gameObject.SetActive(false);
        }
        
        public void OpenPanel()
        {
            _open = true;
            gameObject.SetActive(true);
            optionsParent.DOKill();
            optionsParent.anchoredPosition = new Vector3(0, optionsParent.sizeDelta.y, 0);
            optionsParent.DOAnchorPosY(0, animationDuration);
        }

        public void ClosePanel()
        {
            if (!_open) return;
            _open = false;
            optionsParent.DOKill();
            optionsParent.anchoredPosition = Vector3.zero;
            optionsParent.DOAnchorPosY(optionsParent.sizeDelta.y, animationDuration)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void SetupOptions(IReadOnlyList<(string, Action)> options, Vector2 position)
        {
            _options = options;
            UpdateOptionsList();
            var rectTransform = (RectTransform)transform;
            // Note: transform mouse space to canva space (canva is in camera space)
            var screenPoint = (Vector3) position;
            screenPoint.z = 10.0f; //distance of the plane from the camera
            rectTransform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }

        private void UpdateOptionsList()
        {
            while (_activeElementListItems.TryDequeue(out var item))
            {
                _elementListItemPool.Return(item);
            }
            
            var height = 0;
            foreach (var (option, action) in _options)
            {
                var item = _elementListItemPool.Get();
                item.Setup(option, action);
                height += item.GetHeight();
                _activeElementListItems.Enqueue(item);
            }

            height += 16;
            height = Math.Max(height, 66);
            
            //Note: update the height of the options panel
            UpdateHeight(height);
        }
        
        private void UpdateHeight(int height)
        {
            var sizeDelta = optionsParent.sizeDelta;
            sizeDelta = new Vector2(sizeDelta.x, height);
            optionsParent.sizeDelta = sizeDelta;
            ((RectTransform)transform).sizeDelta = sizeDelta;
        }
        
        private IOptionItemController Factory()
        {
            var option = Instantiate(optionPrefab, optionsParent);
            return option.GetComponent<OptionItemController>();
        }
    }
}
