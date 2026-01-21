using UnityEngine;
using UnityEngine.EventSystems;

namespace com.ethnicthv.chemlab.client.ui.compound
{
    public class CompoundViewDragger : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public Camera mainCamera;
        public Transform cameraBox;
        [SerializeField] private float rotateSpeed = 100f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float zoom = -25f;
        
        public void OnDrag(PointerEventData eventData)
        {
            //Note: rotate the main Camera
            var transform1 = cameraBox;
            var x = eventData.delta.x * rotateSpeed * Mathf.Deg2Rad;
            var y = eventData.delta.y * rotateSpeed * Mathf.Deg2Rad;

            transform1.Rotate(Vector3.up, y);
            transform1.Rotate(Vector3.right, -x);
        }

        public void OnScroll(PointerEventData eventData)
        {
            var zoomValue = eventData.scrollDelta.y;
            zoom += zoomValue * zoomSpeed;

            Transform transform2;
            (transform2 = mainCamera.transform).LookAt(cameraBox);
            transform2.localPosition = new Vector3(0, 0, zoom);
        }
    }
}