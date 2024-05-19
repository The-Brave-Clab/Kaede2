using System;
using System.Collections;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public abstract class TabItem : SelectableItem, IThemeChangeObserver, IPointerExitHandler
    {
        [SerializeField]
        private Image outline;

        [SerializeField]
        private Image foreground;

        [SerializeField]
        private TextMeshProUGUI text;

        [NonSerialized]
        public TabGroup group;

        private bool active;
        public bool Active
        {
            get => active;
            set
            {
                if (value == active) return;
                active = value;
                UpdateColor();
            }
        }

        private Color highlightOutlineColor;
        private Color highlightForegroundColor;
        private Color highlightTextColor;

        private Color activeOutlineColor;
        private Color activeForegroundColor;
        private Color activeTextColor;

        private Color inactiveOutlineColor;
        private Color inactiveForegroundColor;
        private Color inactiveTextColor;

        private Coroutine hoverCoroutine;
        private Sequence hoverSequence;

        private RectTransform rt;

        protected abstract Vector2 GetTargetSizeDelta(Vector2 sizeDelta, bool isSelected);

        protected override void Awake()
        {
            base.Awake();

            OnThemeChange(Theme.Current);

            hoverCoroutine = null;
            hoverSequence = null;

            rt = GetComponent<RectTransform>();

            onSelected.AddListener(OnSelected);
            onDeselected.AddListener(OnDeselected);
            onConfirmed.AddListener(() =>
            {
                if (group != null)
                    group.SelectTab(this);
            });
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightOutlineColor = theme.HoverHighlight;
            highlightForegroundColor = theme.HoverColor;
            highlightTextColor = Color.white;

            activeOutlineColor = new Color(0.2f, 0.2f, 0.2588235f, 1);
            activeForegroundColor = theme.SelectedColor;
            activeTextColor = new Color(0.1921569f, 0.2039216f, 0.2588235f, 1);

            inactiveOutlineColor = new Color(0.3411765f, 0.4117647f, 0.4941177f, 1);
            inactiveForegroundColor = new Color(0.5686275f, 0.6f, 0.6352941f, 1);
            inactiveTextColor = new Color(0.3490196f, 0.3647059f, 0.4196079f, 1);

            UpdateColor();
        }

        private void UpdateColor()
        {
            if (selected)
            {
                outline.color = highlightOutlineColor;
                foreground.color = highlightForegroundColor;
                text.color = highlightTextColor;
            }
            else if (Active)
            {
                outline.color = activeOutlineColor;
                foreground.color = activeForegroundColor;
                text.color = activeTextColor;
            }
            else
            {
                outline.color = inactiveOutlineColor;
                foreground.color = inactiveForegroundColor;
                text.color = inactiveTextColor;
            }
        }

        private void OnSelected()
        {
            StopCurrent();

            Color currentOutlineColor = outline.color;
            Color currentForegroundColor = foreground.color;
            Color currentTextColor = text.color;
            var currentSizeDelta = rt.sizeDelta;
            var targetSizeDelta = GetTargetSizeDelta(currentSizeDelta, true);

            hoverSequence = DOTween.Sequence();
            hoverSequence.Append(DOVirtual.Float(0f, 1f, 0.1f, t =>
            {
                outline.color = Color.Lerp(currentOutlineColor, highlightOutlineColor, t);
                foreground.color = Color.Lerp(currentForegroundColor, highlightForegroundColor, t);
                text.color = Color.Lerp(currentTextColor, highlightTextColor, t);
                rt.sizeDelta = Vector2.Lerp(currentSizeDelta, targetSizeDelta, t);
            }));

            hoverCoroutine = StartCoroutine(RunSequence());
        }

        private void OnDeselected()
        {
            StopCurrent();

            Color currentOutlineColor = outline.color;
            Color currentForegroundColor = foreground.color;
            Color currentTextColor = text.color;
            var currentSizeDelta = rt.sizeDelta;
            var targetSizeDelta = GetTargetSizeDelta(currentSizeDelta, false);

            hoverSequence = DOTween.Sequence();
            hoverSequence.Append(DOVirtual.Float(0f, 1f, 0.1f, t =>
            {
                outline.color = Color.Lerp(currentOutlineColor, Active ? activeOutlineColor : inactiveOutlineColor, t);
                foreground.color = Color.Lerp(currentForegroundColor, Active ? activeForegroundColor : inactiveForegroundColor, t);
                text.color = Color.Lerp(currentTextColor, Active ? activeTextColor : inactiveTextColor, t);
                rt.sizeDelta = Vector2.Lerp(currentSizeDelta, targetSizeDelta, t);
            }));

            hoverCoroutine = StartCoroutine(RunSequence());
        }

        private void StopCurrent()
        {
            if (hoverCoroutine == null) return;

            StopCoroutine(hoverCoroutine);
            hoverSequence.Kill();
            hoverCoroutine = null;
            hoverSequence = null;
        }

        private IEnumerator RunSequence()
        {
            yield return hoverSequence.WaitForCompletion();
            hoverSequence = null;
            hoverCoroutine = null;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onDeselected?.Invoke();
        }
    }
}