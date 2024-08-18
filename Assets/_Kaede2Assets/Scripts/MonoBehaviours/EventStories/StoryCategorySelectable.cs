using System.Collections;
using System.Linq;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Kaede2
{
    class StoryCategorySelectable : SelectableItem
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private Color textOutlineActivatedColor;

        [SerializeField]
        private Color textOutlineDeactivatedColor;

        [SerializeField]
        private bool activated;

        [SerializeField]
        private GameObject notActivatedColorAdjuster;

        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private TextMeshProUGUI textOutline;
        
        [SerializeField]
        private AlbumExtraInfo albumExtraInfo;

        [SerializeField]
        private AlbumExtraInfo.ImageFilter loadFilter = AlbumExtraInfo.ImageFilter.Is16By9;

        private bool lastActivated;
        private AsyncOperationHandle<Sprite> handle;

        public bool Loaded { get; private set; } = false;

        public bool Activated
        {
            get => activated;
            set => activated = value;
        }

        protected override void Awake()
        {
            base.Awake();
            selectedOutline.SetActive(false);
            lastActivated = activated;
            UpdateActivatedStatus();

            onSelected.AddListener(() =>
            {
                selectedOutline.SetActive(true);
            });

            onDeselected.AddListener(() =>
            {
                selectedOutline.SetActive(false);
            });
        }

        private IEnumerator Start()
        {
            var extraInfo = albumExtraInfo.list.Where(i => i.Passes(loadFilter)).OrderBy(_ => Random.value).FirstOrDefault();
            // var info = MasterAlbumInfo.FromAlbumName(extraInfo.name);
            handle = ResourceLoader.LoadIllustration(extraInfo.name);

            yield return handle;

            image.sprite = handle.Result;
            Loaded = true;
        }

        protected override void Update()
        {
            base.Update();
            if (lastActivated != activated)
            {
                lastActivated = activated;

                UpdateActivatedStatus();
                selectedOutline.SetActive(selected);
            }
        }

        private void OnDestroy()
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        private void UpdateActivatedStatus()
        {
            notActivatedColorAdjuster.SetActive(!activated);
            textOutline.color = activated ? textOutlineActivatedColor : textOutlineDeactivatedColor;
        }
    }
}

