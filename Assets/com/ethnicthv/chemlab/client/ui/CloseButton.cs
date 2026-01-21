using UnityEngine;
using UnityEngine.EventSystems;

namespace com.ethnicthv.chemlab.client.ui
{
    public class CloseButton : MonoBehaviour
    {
        public GameObject panel;
        
        public void OnClick()
        {
            panel.SendMessage("ClosePanel");
        }
    }
}