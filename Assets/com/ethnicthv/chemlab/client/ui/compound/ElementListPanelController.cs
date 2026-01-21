using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.ui.compound;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.util.pool;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.compound
{
    public class ElementListPanelController : MonoBehaviour, IElementListPanelController
    {
        [SerializeField] private ElementListOpenButtonController elementListOpenButtonController;
        [SerializeField] private VerticalLayoutGroup elementListLayoutGroup;
        [SerializeField] private GameObject elementListItemPrefab;
        
        private IReadOnlyDictionary<Element, Color> _elementList;
        
        private Pool<IElementListItemController> _elementListItemPool;
        
        private Queue<IElementListItemController> _activeElementListItems;
        
        private float _initialPosition;
        private float _hidePosition;
        
        private void Awake()
        {
            _elementListItemPool = new Pool<IElementListItemController>(Factory);
            _activeElementListItems = new Queue<IElementListItemController>();
        }

        private void Start()
        {
            var transform1 = transform;
            var localPosition = transform1.localPosition;
            
            _initialPosition = localPosition.x;
            _hidePosition = _initialPosition - 600;
            
            //Note: hide the panel on start
            localPosition = new Vector3(_hidePosition, localPosition.y, localPosition.z);
            transform1.localPosition = localPosition;
            
            gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            gameObject.SetActive(true);
            transform.DOLocalMoveX(_initialPosition,0.2f);
        }

        public void ClosePanel()
        {
            elementListOpenButtonController.Show();
            transform.DOLocalMoveX(_hidePosition,0.2f).OnComplete(() => gameObject.SetActive(false));
        }

        public void SetElementList(IReadOnlyDictionary<Element, Color> elementList)
        {
            _elementList = elementList;
            UpdateList();
        }

        private void UpdateList()
        {
            ClearList();
            foreach (var element in _elementList)
            {
                var elementListItem = _elementListItemPool.Get();
                elementListItem.Setup(element.Key, element.Value);
                _activeElementListItems.Enqueue(elementListItem);
            }
        }

        private void ClearList()
        {
            while (_activeElementListItems.TryDequeue(out var elementListItem))
            {
                _elementListItemPool.Return(elementListItem);
            }
        }
        
        private IElementListItemController Factory()
        {
            var elementListItem = Instantiate(elementListItemPrefab, elementListLayoutGroup.transform).GetComponent<ElementListItemController>();
            return elementListItem;
        }
    }
}