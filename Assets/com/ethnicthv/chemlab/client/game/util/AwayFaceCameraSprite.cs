using UnityEngine;

namespace com.ethnicthv.chemlab.client.game.util
{
    public class AwayFaceCameraSprite : MonoBehaviour
    {
        private void Update()
        {
            //Note: get the forward direction of the camera
            Vector3 forward = ClientManager.Instance.mainCamera.transform.forward;

            //Note: rotate the sprite to make it forward parallel to the camera forward
            var transform1 = transform;
            transform1.forward = forward;
            
        }
    }
}