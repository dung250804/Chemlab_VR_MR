using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.util.pool;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.ethnicthv.chemlab.client.ui.storage
{
    public class StoredItemController : MonoBehaviour , IPoolable , IPointerDownHandler
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private IStorable _item;
        
        private float _lastClickTime;
        private const float DoubleClickTime = 0.5f;
        
        public void Setup(IStorable item)
        {
            _item = item;
            if (StorageManager.Instance.TryGetStorageTex(_item.GetStorageTexKey(), out var handle, out var sprite))
            {
                spriteRenderer.sprite = sprite;
            }
            else
            {
                handle.Completed += operation =>
                {
                    spriteRenderer.sprite = operation.Result;
                };
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Time.time - _lastClickTime < DoubleClickTime)
            {
                _lastClickTime = 0;
                StorageManager.Instance.Take(_item);
            }
            else
            {
                _lastClickTime = Time.time;
            }
        }

        public void ResetInstance()
        {
            gameObject.SetActive(false);
            _item = null;
            spriteRenderer.sprite = null;
            StopAllCoroutines();
        }
    }
}