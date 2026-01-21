using UnityEngine;
using UnityEngine.EventSystems;

namespace com.ethnicthv.chemlab.client.ui
{
    public class DraggablePanel : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public RectTransform mainPanel;
        
        private Vector3 _pointerOffset;
        private RectTransform _canvasRectTransform;

        public void OnBeginDrag(PointerEventData eventData)
        {
            var position = mainPanel.position;
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f; //distance of the plane from the camera
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            _pointerOffset = mousePosition - position;
            
            //Note: set mainPanel as LastSibling
            mainPanel.transform.SetAsLastSibling();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (mainPanel == null) return;
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f; //distance of the plane from the camera
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var pointerPosition = ClampToWindow(mousePosition);
            Vector3 position = mousePosition - _pointerOffset;
            mainPanel.position = position;
        }
        
        private Vector3 ClampToWindow(Vector3 mousePosition)
        {
            var rawPointerPosition = mousePosition;
            var localPointerPosition = rawPointerPosition;
            
            var sizeDelta = mainPanel.sizeDelta;
            var halfWidth = sizeDelta.x * 0.5f;
            var halfHeight = sizeDelta.y * 0.5f;
            
            var position = mainPanel.position;
            // position.x = Mathf.Clamp(localPointerPosition.x, -halfWidth, halfWidth);
            // position.y = Mathf.Clamp(localPointerPosition.y, -halfHeight, halfHeight);

            position = localPointerPosition;
            
            return position;
        }
    }
}