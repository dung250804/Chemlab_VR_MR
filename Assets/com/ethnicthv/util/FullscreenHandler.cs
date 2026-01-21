using UnityEngine;

namespace com.ethnicthv.util
{
    public class FullscreenHandler : MonoBehaviour
    {
        private bool _lastFullscreen;
        private int _lastResolutionWidth;
        private int _lastResolutionHeight;

        protected void Awake()
        {
            _lastResolutionWidth = Screen.currentResolution.width;
            _lastResolutionHeight = Screen.currentResolution.height;
            _lastFullscreen = Screen.fullScreen;
        }

        void Update()
        {
            if (Screen.fullScreen != _lastFullscreen)
            {
                if (Screen.fullScreen)
                {
                    Screen.SetResolution(_lastResolutionWidth, _lastResolutionHeight, true);
                }
                _lastFullscreen = Screen.fullScreen;
            }

            if (!Screen.fullScreen && (Screen.currentResolution.width != _lastResolutionWidth || Screen.currentResolution.height != _lastResolutionHeight))
            {
                _lastResolutionWidth = Screen.currentResolution.width;
                _lastResolutionHeight = Screen.currentResolution.height;
            }
        }
    }
}