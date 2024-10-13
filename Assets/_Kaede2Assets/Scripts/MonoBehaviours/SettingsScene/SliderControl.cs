using System.Collections;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kaede2
{
    [ExecuteAlways]
    public class SliderControl : SettingsControl, IThemeChangeObserver
    {
        [SerializeField]
        private SliderHandle handle;
    
        [SerializeField]
        private RectTransform fill;

        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private Image backgroundBody;

        [SerializeField]
        private Image backgroundOutline;

        [SerializeField]
        [Range(0, 1)]
        private float value;

        [SerializeField]
        private UnityEvent<float> onValueChanged;

        [SerializeField]
        private UnityEvent<float> onValueChangeEnd;

        private RectTransform rt;
        private RectTransform handleRT;

        private static readonly Color FillDeactivatedColor = new Color(0.7764706f, 0.7803922f, 0.7764706f, 1f);
        private static readonly Color BackgroundBodyDeactivatedColor = new Color(0.3803922f, 0.3803922f, 0.3803922f, 1f);
        private static readonly Color BackgroundOutlineDeactivatedColor = new Color(0.04313726f, 0.04313726f, 0.04313726f, 1f);
        private static readonly Color BackgroundBodyActivatedColor = new Color(0.5294118f, 0.5294118f, 0.5294118f, 1f);
        private static readonly Color BackgroundOutlineActivatedColor = new Color(0.3411765f, 0.3411765f, 0.3411765f, 1f);

        private Color fillActivatedColor;

        private Coroutine appearanceUpdateCoroutine;
        private Sequence appearanceUpdateSequence;

        public float Value
        {
            get => value;
            set => OnValueChanged(value);
        }

        protected override void Awake()
        {
            base.Awake();

            rt = GetComponent<RectTransform>();
            handleRT = handle.GetComponent<RectTransform>();

            OnThemeChange(Theme.Current);
        }

        private void Update()
        {
            fill.sizeDelta = new Vector2(handleRT.anchoredPosition.x, fill.sizeDelta.y);

            var handlePos = handleRT.localPosition;
            handlePos.x = (Mathf.Clamp01(value) - 0.5f) * rt.rect.width;
            handleRT.localPosition = handlePos;
        }

        protected override void OnActivate()
        {
            if (!Application.isPlaying)
                return;

            UpdateAppearance(false);
        }

        protected override void OnDeactivate()
        {
            if (!Application.isPlaying)
                return;

            UpdateAppearance(false);
        }

        private void OnValueChanged(float newValue)
        {
            value = newValue;

            if (Application.isPlaying)
                onValueChanged?.Invoke(value);
        }

        public void OnValueChangeEnd()
        {
            if (Application.isPlaying)
                onValueChangeEnd?.Invoke(value);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            if (!Application.isPlaying) return;

            fillActivatedColor = theme.SliderControlActiveFillColor;

            UpdateAppearance(true);
        }

        public void SetValueDisplayOnly(float newValue)
        {
            value = newValue;
        }

        private void UpdateAppearance(bool immediate)
        {
            var currentFillColor = fillImage.color;
            var targetFillColor = activated ? fillActivatedColor : FillDeactivatedColor;

            var currentBackgroundBodyColor = backgroundBody.color;
            var targetBackgroundBodyColor = activated ? BackgroundBodyActivatedColor : BackgroundBodyDeactivatedColor;

            var currentBackgroundOutlineColor = backgroundOutline.color;
            var targetBackgroundOutlineColor = activated ? BackgroundOutlineActivatedColor : BackgroundOutlineDeactivatedColor;

            if (immediate)
            {
                fillImage.color = targetFillColor;
                backgroundBody.color = targetBackgroundBodyColor;
                backgroundOutline.color = targetBackgroundOutlineColor;
            }
            else
            {
                if (appearanceUpdateCoroutine != null)
                {
                    StopCoroutine(appearanceUpdateCoroutine);
                    appearanceUpdateSequence.Kill();
                    appearanceUpdateCoroutine = null;
                    appearanceUpdateSequence = null;
                }

                IEnumerator UpdateAppearanceCoroutine()
                {
                    appearanceUpdateSequence = DOTween.Sequence();

                    appearanceUpdateSequence.Append(DOVirtual.Float(0, 1, 0.1f, t =>
                    {
                        if (fillImage != null)
                            fillImage.color = Color.Lerp(currentFillColor, targetFillColor, t);

                        if (backgroundBody != null)
                            backgroundBody.color = Color.Lerp(currentBackgroundBodyColor, targetBackgroundBodyColor, t);

                        if (backgroundOutline != null)
                            backgroundOutline.color = Color.Lerp(currentBackgroundOutlineColor, targetBackgroundOutlineColor, t);
                    }));

                    yield return appearanceUpdateSequence.WaitForCompletion();

                    appearanceUpdateSequence = null;
                    appearanceUpdateCoroutine = null;
                }

                appearanceUpdateCoroutine = StartCoroutine(UpdateAppearanceCoroutine());
            }
        }

        public override void Left()
        {
            value -= 0.1f;
            value = Mathf.Clamp01(value);
            OnValueChanged(value);
            OnValueChangeEnd();
        }

        public override void Right()
        {
            value += 0.1f;
            value = Mathf.Clamp01(value);
            OnValueChanged(value);
            OnValueChangeEnd();
        }
    }
}