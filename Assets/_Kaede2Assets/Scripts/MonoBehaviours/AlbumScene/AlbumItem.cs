using System.Linq;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
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

        [SerializeField]
        private FavoriteIcon wallpaperIcon;

        [SerializeField]
        private TMP_FontAsset titleFont;

        public UnityEvent onSelected;

        public FavoriteIcon FavoriteIcon => favoriteIcon;
        public FavoriteIcon WallpaperIcon => wallpaperIcon;

        private AsyncOperationHandle<Sprite> handle;

        private MasterAlbumInfo.AlbumInfo albumInfo;

        public MasterAlbumInfo.AlbumInfo AlbumInfo
        {
            get => albumInfo;
            set
            {
                albumInfo = value;
                if (albumInfo != null && SaveData.MainMenuBackground.AlbumName == albumInfo.AlbumName)
                    currentWallpaper = this;
            }
        }

        private static AlbumItem currentSelected = null;
        public static AlbumItem CurrentSelected => currentSelected;

        private static AlbumItem currentWallpaper = null;

        private static RectTransform viewportRT;
        private static Vector3[] viewportCorners;
        private static Vector2 viewportYMinMax;
        // in reality viewport RT should not change, so we actually don't need to update this every frame
        private static int viewportCornersFrame;

        private RectTransform rt;
        private Vector3[] corners;

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

        private bool IsFavorite
        {
            get => SaveData.FavoriteAlbumNames.Any(n => n == AlbumInfo.AlbumName);
            set
            {
                if (IsFavorite == value) return;

                if (value)
                {
                    SaveData.AddFavoriteAlbum(AlbumInfo);
                    AudioManager.ConfirmSound();
                }
                else
                {
                    SaveData.RemoveFavoriteAlbum(AlbumInfo);
                    AudioManager.CancelSound();
                }
            }
        }

        private bool IsWallpaper
        {
            get => SaveData.MainMenuBackground.AlbumName == AlbumInfo.AlbumName;
            set
            {
                if (!value) return;
                if (IsWallpaper) return;

                SaveData.MainMenuBackground = AlbumInfo;
                if (currentWallpaper != null)
                    currentWallpaper.wallpaperIcon.UpdateColor();
                currentWallpaper = this;
                AudioManager.ConfirmSound();
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

            InputManager.onDeviceTypeChanged += OnInputDeviceChanged;
        }

        private void OnDestroy()
        {
            InputManager.onDeviceTypeChanged -= OnInputDeviceChanged;
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

            favoriteIcon.OnClicked = () => { IsFavorite = !IsFavorite; };
            favoriteIcon.IsFavorite = () => IsFavorite;

            wallpaperIcon.OnClicked = () => { IsWallpaper = true; };
            wallpaperIcon.IsFavorite = () => IsWallpaper;
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

        public void UpdateSelectionVisibleStatus(bool selected)
        {
            selectedOutline.SetActive(InputManager.CurrentDeviceType != InputDeviceType.Touchscreen && selected);
            favoriteIcon.gameObject.SetActive((InputManager.CurrentDeviceType == InputDeviceType.Touchscreen && visible) || selected);
            wallpaperIcon.gameObject.SetActive((InputManager.CurrentDeviceType == InputDeviceType.Touchscreen && visible) || selected);
        }

        public void Select(bool makeSureFullyVisible)
        {
            if (currentSelected != this && currentSelected != null)
            {
                currentSelected.Deselect();
            }

            currentSelected = this;
            UpdateSelectionVisibleStatus(true);
            AlbumTitle.Text = AlbumInfo.ViewName;
            AlbumTitle.Font = titleFont;
            onSelected.Invoke();

            if (makeSureFullyVisible)
                Scroll.MoveItemIntoViewport(rt, 0.1f);
        }

        public void Deselect()
        {
            UpdateSelectionVisibleStatus(false);
        }

        private void OnInputDeviceChanged(InputDeviceType type)
        {
            UpdateSelectionVisibleStatus(currentSelected == this);
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

            AudioManager.ConfirmSound();
            AlbumItemViewCanvas.Enter(viewItem);
        }
    }
}