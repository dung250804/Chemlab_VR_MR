using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.element
{
    public class ElementDisplayer : MonoBehaviour
    {
        [SerializeField] private GameObject electronPrefab;
        [SerializeField] private GameObject pathPrefab;
        [SerializeField] private RectTransform electronContainer;
        [SerializeField] private RectTransform pathContainer;
        [SerializeField] private TextMeshProUGUI elementSymbolText;
        
        public void SetupDisplay(string elementSymbol, Dictionary<int, int> electronConfiguration)
        {
            elementSymbolText.text = elementSymbol;
            
            foreach (Transform child in electronContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (Transform child in pathContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var (n, e) in electronConfiguration)
            {
                var anglePerStep = 360f / e;
                var distance = 50 + 40 * n;
                
                for (var i = 0; i < e; i++)
                {
                    var electron = Instantiate(electronPrefab, electronContainer);
                    electron.transform.localPosition = new Vector3(
                        distance * Mathf.Cos(Mathf.Deg2Rad * anglePerStep * i), 
                        distance * Mathf.Sin(Mathf.Deg2Rad * anglePerStep * i), 
                        0);
                }
                
                var path = Instantiate(pathPrefab, pathContainer);
                var pathTransform = (RectTransform) path.transform;
                pathTransform.sizeDelta = new Vector2(distance * 2, distance * 2);
            }
        }
    }
}
