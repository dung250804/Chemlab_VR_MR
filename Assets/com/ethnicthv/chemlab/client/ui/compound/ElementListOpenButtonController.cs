using com.ethnicthv.chemlab.client.api.ui.compound;
using DG.Tweening;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.compound
{
    public class ElementListOpenButtonController : MonoBehaviour, IElementListOpenButtonController
    {
        [SerializeField] private ElementListPanelController elementListPanelController;
        
        private float _initialPosition;
        
        private void Start()
        {
            _initialPosition = transform.localPosition.x;
        }
        
        public void OnClick()
        {
            elementListPanelController.OpenPanel();
            transform.DOLocalMoveX(-200,0.2f);
        }

        public void Show()
        {
            transform.DOLocalMoveX(_initialPosition,0.2f);
        }
    }
}