using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class StoryCategorySelectable : SelectableItem
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private RectTransform imageContainer;

        [SerializeField]
        private ColorAdjustmentMask notActivatedColorAdjuster;

        [SerializeField]
        private Color textOutlineActivatedColor;

        [SerializeField]
        private Color textOutlineDeactivatedColor;

        [SerializeField]
        [Range(-1, 1)]
        private float deactivatedHueAdjustment;

        [SerializeField]
        [Range(-1, 1)]
        private float deactivatedSaturationAdjustment;

        [SerializeField]
        [Range(-1, 1)]
        private float deactivatedLightnessAdjustment;

        [SerializeField]
        [ColorUsage(false)]
        private Color deactivatedColorTintAdjustment = Color.white;

        [SerializeField]
        private bool activated;

        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private TextMeshProUGUI textOutline;

        [SerializeField]
        private RandomizedImageProvider imageProvider;

        [SerializeField]
        private UnityEvent<Color> onTextOutlineColorChanged;

        private bool lastActivated;
        private AsyncOperationHandle<Sprite> handle;

        public bool Loaded { get; private set; } = false;
        public string Text => textOutline.text;

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
            notActivatedColorAdjuster.Hue = activated ? 0 : deactivatedHueAdjustment;
            notActivatedColorAdjuster.Saturation = activated ? 0 : deactivatedSaturationAdjustment;
            notActivatedColorAdjuster.Lightness = activated ? 0 : deactivatedLightnessAdjustment;
            notActivatedColorAdjuster.Color = activated ? Color.white : deactivatedColorTintAdjustment;
            var initialOutlineColor = activated ? textOutlineActivatedColor : textOutlineDeactivatedColor;
            textOutline.color = initialOutlineColor;
            onTextOutlineColorChanged.Invoke(initialOutlineColor);

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
            yield return Refresh();
            Loaded = true;
        }

        public IEnumerator Refresh()
        {
            yield return imageProvider.Provide(1, infos =>
            {
                if (infos.Length == 0)
                {
                    this.LogError("No images available for event story");
                    return;
                }
                image.sprite = infos[0].Sprite;
            });
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
            var startHue = notActivatedColorAdjuster.Hue;
            var startSaturation = notActivatedColorAdjuster.Saturation;
            var startLightness = notActivatedColorAdjuster.Lightness;
            var startColor = notActivatedColorAdjuster.Color;
            var startOutlineColor = textOutline.color;

            var targetHue = activated ? 0 : deactivatedHueAdjustment;
            var targetSaturation = activated ? 0 : deactivatedSaturationAdjustment;
            var targetLightness = activated ? 0 : deactivatedLightnessAdjustment;
            var targetColor = activated ? Color.white : deactivatedColorTintAdjustment;
            var targetOutlineColor = activated ? textOutlineActivatedColor : textOutlineDeactivatedColor;

            activatedSequence = DOTween.Sequence();
            activatedSequence.Append(DOVirtual.Float(0, 1, 0.2f,
                value =>
                {
                    notActivatedColorAdjuster.Hue = Mathf.Lerp(startHue, targetHue, value);
                    notActivatedColorAdjuster.Saturation = Mathf.Lerp(startSaturation, targetSaturation, value);
                    notActivatedColorAdjuster.Lightness = Mathf.Lerp(startLightness, targetLightness, value);
                    notActivatedColorAdjuster.Color = Color.Lerp(startColor, targetColor, value);
                    Color outlineColor = Color.Lerp(startOutlineColor, targetOutlineColor, value);
                    textOutline.color = outlineColor;
                    onTextOutlineColorChanged.Invoke(outlineColor);
                }));

            yield return activatedSequence.WaitForCompletion();

            activatedCoroutine = null;
            activatedSequence = null;
        }
    }
}

