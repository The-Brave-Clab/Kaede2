using System.Collections;
using System.Linq;
using DG.Tweening;
using Kaede2.Scenario.Framework.Utils;
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
        private static readonly int SaturationAdjustment = Shader.PropertyToID("_SaturationAdjustment");

        [SerializeField]
        private Image image;

        [SerializeField]
        private RectTransform imageContainer;

        [SerializeField]
        private Color textOutlineActivatedColor;

        [SerializeField]
        private Color textOutlineDeactivatedColor;

        [SerializeField]
        private bool activated;

        [SerializeField]
        private Image notActivatedColorAdjuster;

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

        private Coroutine selectedCoroutine;
        private Sequence selectedSequence;

        private Coroutine activatedCoroutine;
        private Sequence activatedSequence;

        protected override void Awake()
        {
            base.Awake();
            selectedOutline.SetActive(false);
            lastActivated = activated;
            notActivatedColorAdjuster.material.SetFloat(SaturationAdjustment, activated ? 0 : -0.5f);
            textOutline.color = activated ? textOutlineActivatedColor : textOutlineDeactivatedColor;

            onSelected.AddListener(() =>
            {
                UpdateSelectedStatus(true);
            });

            onDeselected.AddListener(() =>
            {
                UpdateSelectedStatus(false);
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
                UpdateSelectedStatus(selected);
            }
        }

        private void OnDestroy()
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        private void UpdateSelectedStatus(bool selected)
        {
            if (selectedCoroutine != null)
            {
                StopCoroutine(selectedCoroutine);
                selectedSequence.Kill();
                selectedCoroutine = null;
                selectedSequence = null;
            }

            selectedCoroutine = CoroutineProxy.Start(ChangeSelectedStatus(selected));
        }

        private IEnumerator ChangeSelectedStatus(bool selected)
        {
            selectedOutline.SetActive(selected);

            var startScale = imageContainer.localScale;
            var targetScale = Vector3.one * (selected ? 1.05f : 1.0f);

            selectedSequence = DOTween.Sequence();
            selectedSequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
            {
                imageContainer.localScale = Vector3.Lerp(startScale, targetScale, value);
            }));

            yield return selectedSequence.WaitForCompletion();

            selectedCoroutine = null;
            selectedSequence = null;
        }

        private void UpdateActivatedStatus()
        {
            if (activatedCoroutine != null)
            {
                StopCoroutine(activatedCoroutine);
                activatedSequence.Kill();
                activatedCoroutine = null;
                activatedSequence = null;
            }

            activatedCoroutine = CoroutineProxy.Start(ChangeActivatedStatus());
        }

        private IEnumerator ChangeActivatedStatus()
        {
            var startSaturation = notActivatedColorAdjuster.material.GetFloat(SaturationAdjustment);
            var startOutlineColor = textOutline.color;

            var targetSaturation = activated ? 0 : -0.5f;
            var targetOutlineColor = activated ? textOutlineActivatedColor : textOutlineDeactivatedColor;

            activatedSequence = DOTween.Sequence();
            activatedSequence.Append(DOVirtual.Float(0, 1, 0.2f,
                value =>
                {
                    notActivatedColorAdjuster.material.SetFloat(SaturationAdjustment,
                        Mathf.Lerp(startSaturation, targetSaturation, value));
                    textOutline.color = Color.Lerp(startOutlineColor, targetOutlineColor, value);
                }));

            yield return activatedSequence.WaitForCompletion();

            activatedCoroutine = null;
            activatedSequence = null;
        }
    }
}

