using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kaede2
{
    public class SelectionControl : SettingsControl, IThemeChangeObserver
    {
        [SerializeField]
        private Image leftArrow;

        [SerializeField]
        private Image rightArrow;

        [SerializeField]
        private RectTransform itemContainer;

        [SerializeField]
        private RectTransform itemViewport;

        [SerializeField]
        private GameObject itemPrefab;

        [SerializeField]
        private List<SelectionItem> items;

        [SerializeField]
        private bool loop = true;

        private static readonly Color DeactivatedArrowColor = new(0.7411765f, 0.7568628f, 0.7803922f);
        private static readonly Color DeactivatedTextColor = new(0.3490196f, 0.3647059f, 0.4196078f);
        private static readonly Color ActivatedTextColor = new(0.1686275f, 0.1843137f, 0.2313726f);
        private Color activatedArrowColor;

        private Color currentTextColor;

        private int selectedIndex = 0;

        private Coroutine activateCoroutine;
        private Sequence activateSequence;

        private Coroutine selectCoroutine;
        private Sequence selectSequence;

        protected override void Awake()
        {
            base.Awake();

            currentTextColor = DeactivatedTextColor;
            itemContainer.sizeDelta = itemViewport.rect.size;

            OnThemeChange(Theme.Current);
        }

        protected override void OnActivate()
        {
            StopCurrentActivateCoroutine();

            activateCoroutine = StartCoroutine(ActivateCoroutine(true));
        }

        protected override void OnDeactivate()
        {
            StopCurrentActivateCoroutine();

            activateCoroutine = StartCoroutine(ActivateCoroutine(false));
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            activatedArrowColor = theme.HoverHighlight;

            UpdateAppearance();
        }

        public void SelectImmediate(int index, bool triggerEvent)
        {
            selectedIndex = loop ? CommonUtils.Mod(index, items.Count) : Mathf.Clamp(index, 0, items.Count - 1);
            itemContainer.anchoredPosition = new Vector2(CalculateTargetPosition(selectedIndex), itemContainer.anchoredPosition.y);

            if (triggerEvent)
                items[selectedIndex].Select();
        }

        public void Select(int indexDiff)
        {
            if (items.Count == 0) return;
            if (indexDiff == 0) return;

            int nextIndex = selectedIndex + indexDiff;
            nextIndex = loop ? CommonUtils.Mod(nextIndex, items.Count) : Mathf.Clamp(nextIndex, 0, items.Count - 1);

            if (nextIndex == selectedIndex) return;

            StopCurrentSelectCoroutine();

            selectCoroutine = StartCoroutine(SelectCoroutine(nextIndex));
        }

        public SelectionItem Add(string text, UnityAction action)
        {
            GameObject itemObject = Instantiate(itemPrefab, itemContainer);
            itemObject.name = text;
            SelectionItem item = itemObject.GetComponent<SelectionItem>();

            item.Text = text;
            item.OnSelected.AddListener(action);

            items.Add(item);

            UpdateAppearance();

            return item;
        }

        public void Clear()
        {
            foreach (var item in items)
                Destroy(item.gameObject);

            items.Clear();
        }

        private float CalculateTargetPosition(int index)
        {
            return -index * itemViewport.rect.width;
        }

        private void UpdateAppearance()
        {
            leftArrow.color = activated ? activatedArrowColor : DeactivatedArrowColor;
            rightArrow.color = activated ? activatedArrowColor : DeactivatedArrowColor;

            currentTextColor = activated ? ActivatedTextColor : DeactivatedTextColor;

            foreach (var item in items)
            {
                item.Color = currentTextColor;
                item.RectTransform.sizeDelta = itemViewport.rect.size;
            }
        }

        private void StopCurrentActivateCoroutine()
        {
            if (activateCoroutine == null) return;
            StopCoroutine(activateCoroutine);
            activateSequence.Kill();
            activateSequence = null;
            activateCoroutine = null;
        }

        private IEnumerator ActivateCoroutine(bool activate)
        {
            Color startArrowColor = leftArrow.color;
            Color endArrowColor = activate ? activatedArrowColor : DeactivatedArrowColor;

            Color startTextColor = currentTextColor;
            Color endTextColor = activate ? ActivatedTextColor : DeactivatedTextColor;

            activateSequence = DOTween.Sequence();

            activateSequence.Append(DOVirtual.Float(0, 1, 0.1f, t =>
            {
                leftArrow.color = Color.Lerp(startArrowColor, endArrowColor, t);
                rightArrow.color = Color.Lerp(startArrowColor, endArrowColor, t);

                currentTextColor = Color.Lerp(startTextColor, endTextColor, t);

                foreach (var item in items)
                    item.Color = currentTextColor;
            }));

            yield return activateSequence.WaitForCompletion();

            activateSequence = null;
            activateCoroutine = null;
        }

        private void StopCurrentSelectCoroutine()
        {
            if (selectCoroutine == null) return;
            StopCoroutine(selectCoroutine);
            selectSequence.Kill();
            selectSequence = null;
            selectCoroutine = null;
        }

        private IEnumerator SelectCoroutine(int nextIndex)
        {
            float targetPosition = CalculateTargetPosition(nextIndex);

            selectedIndex = nextIndex;

            items[selectedIndex].Select();

            selectSequence = DOTween.Sequence();

            selectSequence.Append(DOVirtual.Float(itemContainer.anchoredPosition.x, targetPosition, 0.2f, value =>
            {
                itemContainer.anchoredPosition = new Vector2(value, itemContainer.anchoredPosition.y);
            }));

            yield return selectSequence.WaitForCompletion();

            selectSequence = null;
            selectCoroutine = null;
        }
    }
}