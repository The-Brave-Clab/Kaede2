using System.Linq;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class AlbumItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private FavoriteIcon favoriteIcon;

        private AsyncOperationHandle<Sprite> handle;

        public MasterAlbumInfo.AlbumInfo AlbumInfo { get; set; }

        private static AlbumItem currentSelected = null;
        public static AlbumItem CurrentSelected => currentSelected;

        private static RectTransform viewportRT;
        private static Vector3[] viewportCorners;
        private static Vector2 viewportYMinMax;
        // in reality viewport RT should not change, so we actually don't need to update this every frame
        private static int viewportCornersFrame;

        private RectTransform rt;
        private Vector3[] corners;

        public RectTransform RectTransform => rt;
        public RectTransform Viewport => viewportRT;
        public Vector3[] WorldCorners => corners;
        public static Vector3[] ViewportWorldCorners => viewportCorners;

        private const float ViewportYMultiplier = 20.0f;


        private bool visible;

        // we need to skip for the first frame where the object is created/enabled
        // since the grid layout group will not have updated the position yet
        // leading to the wrong visibility state
        private bool skipUpdate;

        public ScrollRect Scroll { get; set; }

        private bool Visible
        {
            get => visible;
            set
            {
                if (visible == value) return;

                visible = value;
                if (visible)
                {
                    Load();
                }
                else
                {
                    Unload();
                }
            }
        }

        public bool IsFavorite
        {
            get => SaveData.FavoriteAlbumNames.Any(n => n == AlbumInfo.AlbumName);
            set
            {
                if (IsFavorite == value) return;

                if (value)
                    SaveData.AddFavoriteAlbum(AlbumInfo);
                else
                    SaveData.RemoveFavoriteAlbum(AlbumInfo);
            }
        }

        private void Awake()
        {
            AlbumInfo = null;
            handle = new();

            if (viewportRT == null) viewportRT = transform.parent.parent.GetComponent<RectTransform>();
            viewportCorners ??= new Vector3[4];
            viewportCornersFrame = -1;

            rt = GetComponent<RectTransform>();
            corners = new Vector3[4];

            visible = false;
            skipUpdate = true;
        }

        private void Update()
        {
            if (skipUpdate)
            {
                skipUpdate = false;
                return;
            }

            if (viewportCornersFrame != Time.frameCount)
            {
                viewportCornersFrame = Time.frameCount;
                viewportRT.GetWorldCorners(viewportCorners);
                viewportYMinMax = new(Mathf.Min(viewportCorners[0].y, viewportCorners[1].y), Mathf.Max(viewportCorners[0].y, viewportCorners[1].y));
                var viewportYCenter = (viewportYMinMax.x + viewportYMinMax.y) / 2;
                var viewportYExtent = (viewportYMinMax.y - viewportYMinMax.x) / 2;
                viewportYMinMax = new(viewportYCenter - viewportYExtent * ViewportYMultiplier, viewportYCenter + viewportYExtent * ViewportYMultiplier);
            }

            rt.GetWorldCorners(corners);
            Vector2 yMinMax = new(Mathf.Min(corners[0].y, corners[1].y), Mathf.Max(corners[0].y, corners[1].y));

            Visible = yMinMax.y > viewportYMinMax.x && yMinMax.x < viewportYMinMax.y;
        }

        private void OnEnable()
        {
            skipUpdate = true;
            Load();
        }

        private void OnDisable()
        {
            Unload();

            if (currentSelected == this)
            {
                AlbumTitle.Text = "";
            }
            Deselect();
        }

        private void Load()
        {
            if (!Visible) return;
            if (!isActiveAndEnabled) return;
            if (handle.IsValid()) return;
            if (AlbumInfo == null) return;

            handle = ResourceLoader.LoadIllustration(AlbumInfo.AlbumName, true);
            handle.Completed += h =>
            {
                image.sprite = h.Result;
            };
        }

        private void Unload()
        {
            if (!handle.IsValid()) return;

            void UnloadAction()
            {
                image.sprite = null;
                Addressables.Release(handle);
                handle = default;
            }

            if (handle.IsDone)
                UnloadAction();
            else
                handle.Completed += _ => UnloadAction();
        }

        public void Select(bool makeSureFullyVisible)
        {
            if (currentSelected != null)
            {
                currentSelected.Deselect();
            }

            currentSelected = this;
            selectedOutline.SetActive(true);
            favoriteIcon.gameObject.SetActive(true);
            AlbumTitle.Text = AlbumInfo.ViewName;

            if (makeSureFullyVisible)
                Scroll.MoveItemIntoViewport(rt, 0.1f);
        }

        private void Deselect()
        {
            if (currentSelected == this)
            {
                currentSelected = null;
            }

            selectedOutline.SetActive(false);
            favoriteIcon.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var viewItem = AlbumItemViewCanvas.AddItem();
            viewItem.gameObject.name = AlbumInfo.AlbumName;
            viewItem.Image = image.sprite;
            viewItem.Item = this;
            viewItem.Load();

            AlbumItemViewCanvas.Enter(viewItem);
        }
    }
}