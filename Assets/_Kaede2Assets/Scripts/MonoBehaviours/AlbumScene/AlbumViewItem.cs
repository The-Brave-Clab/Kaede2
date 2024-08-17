using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class AlbumViewItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter;

        private RectTransform rt;
        public RectTransform RectTransform => rt;

        private AsyncOperationHandle<Sprite> handle;

        public AlbumItem Item { get; set; }

        public Sprite Image
        {
            get => image.sprite;
            set => image.sprite = value;
        }

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            handle = default;
        }

        public void Load()
        {
            if (handle.IsValid()) Addressables.Release(handle);

            handle = ResourceLoader.LoadIllustration(Item.AlbumInfo.AlbumName);
            handle.Completed += h =>
            {
                image.sprite = h.Result;
            };
        }

        private void OnDestroy()
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }

        private float pointerDownTime;
        private Vector2 pointerDownPosition;
        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownTime = Time.time;
            pointerDownPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            float pointerUpTime = Time.time;
            Vector2 pointerUpPosition = eventData.position;
            
            if (pointerUpTime - pointerDownTime < 0.2f && (pointerDownPosition - pointerUpPosition).magnitude < 0.01f)
            {
                AlbumItemViewCanvas.Instance.ToggleUI();
            }
        }
    }
}
