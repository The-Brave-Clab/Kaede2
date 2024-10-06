using System.Collections;
using System.Linq;
using DG.Tweening;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
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
        private static readonly int HueAdjustment = Shader.PropertyToID("_HueAdjustment");
        private static readonly int SaturationAdjustment = Shader.PropertyToID("_SaturationAdjustment");
        private static readonly int ValueAdjustment = Shader.PropertyToID("_ValueAdjustment");

        [SerializeField]
        private Image image;

        [SerializeField]
        private RectTransform imageContainer;

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
        private float deactivatedValueAdjustment;

        [SerializeField]
        private bool activated;

        [SerializeField]
        private Image notActivatedColorAdjuster;

        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private TextMeshProUGUI textOutline;

        [SerializeField]
        private RandomizedImageProvider imageProvider;

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
            notActivatedColorAdjuster.material.SetFloat(HueAdjustment, activated ? 0 : deactivatedHueAdjustment);
            notActivatedColorAdjuster.material.SetFloat(SaturationAdjustment, activated ? 0 : deactivatedSaturationAdjustment);
            notActivatedColorAdjuster.material.SetFloat(ValueAdjustment, activated ? 0 : deactivatedValueAdjustment);
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
            yield return imageProvider.Provide(1, infos =>
            {
                if (infos.Length == 0)
                {
                    this.LogError("No images available for event story");
                    return;
                }
                image.sprite = infos[0].Sprite;
            });
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
            var startHue = notActivatedColorAdjuster.material.GetFloat(HueAdjustment);
            var startSaturation = notActivatedColorAdjuster.material.GetFloat(SaturationAdjustment);
            var startValue = notActivatedColorAdjuster.material.GetFloat(ValueAdjustment);
            var startOutlineColor = textOutline.color;

            var targetHue = activated ? 0 : deactivatedHueAdjustment;
            var targetSaturation = activated ? 0 : deactivatedSaturationAdjustment;
            var targetValue = activated ? 0 : deactivatedValueAdjustment;
            var targetOutlineColor = activated ? textOutlineActivatedColor : textOutlineDeactivatedColor;

            activatedSequence = DOTween.Sequence();
            activatedSequence.Append(DOVirtual.Float(0, 1, 0.2f,
                value =>
                {
                    notActivatedColorAdjuster.material.SetFloat(HueAdjustment,
                        Mathf.Lerp(startHue, targetHue, value));
                    notActivatedColorAdjuster.material.SetFloat(SaturationAdjustment,
                        Mathf.Lerp(startSaturation, targetSaturation, value));
                    notActivatedColorAdjuster.material.SetFloat(ValueAdjustment,
                        Mathf.Lerp(startValue, targetValue, value));
                    textOutline.color = Color.Lerp(startOutlineColor, targetOutlineColor, value);
                }));

            yield return activatedSequence.WaitForCompletion();

            activatedCoroutine = null;
            activatedSequence = null;
        }
    }
}

