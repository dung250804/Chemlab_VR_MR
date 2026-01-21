using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.alltools
{
    public class AllToolListController : MonoBehaviour
    {
        
        [SerializeField] private AllToolsItemController itemPrefab;
        [SerializeField] private Transform itemContainer;
        
        [SerializedDictionary("Type", "Prefab")] 
        public SerializedDictionary<string, GameObject> _tools;

        private void Awake()
        {
            Setup();
        }

        private void Setup()
        {
            foreach (var (type, prefab) in _tools)
            {
                CreateItem(type, prefab);
            }
        }
        
        private void CreateItem(string type, GameObject prefab)
        {
            var item = Instantiate(itemPrefab, itemContainer);
            item.Set(type, prefab);
            item.gameObject.SetActive(true);
        }
    }
}