using UnityEngine;

namespace com.ethnicthv.util
{
    [RequireComponent(typeof(Camera))]
    public class CameraScaler : MonoBehaviour
    {
        [SerializeField] protected int targetWidth = 1920;
        [SerializeField] protected int targetHeight = 1080;

        [SerializeField] protected int dynamicMaxWidth = 2560;
        [SerializeField] protected int dynamicMaxHeight = 1440;

        [SerializeField] protected bool useDynamicWidth = false;
        [SerializeField] protected bool useDynamicHeight = false;

        private Camera _cam;
        private int _lastWidth = 0;
        private int _lastHeight = 0;

        private float _orthoSize;

        protected void Awake()
        {
            _cam = GetComponent<Camera>();
            _orthoSize = _cam.orthographicSize;
        }

        protected void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                UpdateCamSize();
                _lastWidth = Screen.width;
                _lastHeight = Screen.height;
            }
        }

        private void UpdateCamSize()
        {
            float targetAspect;
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float ortoScale = 1f;

            if (useDynamicWidth)
            {
                float minTargetAspect = (float)targetWidth / (float)targetHeight;
                float maxTargetAspect = (float)dynamicMaxWidth / (float)targetHeight;
                targetAspect = Mathf.Clamp(screenAspect, minTargetAspect, maxTargetAspect);
            }
            else
            {
                targetAspect = (float)targetWidth / (float)targetHeight;
            }

            float scaleValue = screenAspect / targetAspect;

            Rect rect = new();
            if (scaleValue < 1f)
            {
                if (useDynamicHeight)
                {
                    float minTargetAspect = (float)targetWidth / (float)dynamicMaxHeight;
                    if (screenAspect < minTargetAspect)
                    {
                        scaleValue = screenAspect / minTargetAspect;
                        ortoScale = minTargetAspect / targetAspect;
                    }
                    else
                    {
                        ortoScale = scaleValue;
                        scaleValue = 1f;
                    }
                }

                rect.width = 1;
                rect.height = scaleValue;
                rect.x = 0;
                rect.y = (1 - scaleValue) / 2;
            }
            else
            {
                scaleValue = 1 / scaleValue;
                rect.width = scaleValue;
                rect.height = 1;
                rect.x = (1 - scaleValue) / 2;
                rect.y = 0;
            }

            _cam.orthographicSize = _orthoSize / ortoScale;
            _cam.rect = rect;
        }
    }
}