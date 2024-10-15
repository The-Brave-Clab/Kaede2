using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class LabeledListSelectableGroup : SelectableGroup
    {
        [SerializeField]
        private RectTransform highlight;

        [SerializeField]
        private LabeledListSelectableItem itemPrefab;

        [SerializeField]
        private RectTransform contentGroup;

        [SerializeField]
        private LayoutGroup layoutGroup;

        [SerializeField]
        private SelectableGroup additionalSelectableGroup;

        // won't be reset by DeselectAll
        public int LastSelected { get; private set; }

        private bool focusedOnLabeledSelectables;
        public bool FocusedOnLabeledSelectables => focusedOnLabeledSelectables;

        private RectTransform highlightRT;

        private ScrollRect scrollRect;

        private Coroutine coroutine;
        private Sequence sequence;

        private Coroutine scrollCoroutine;

        private Coroutine deselectAllCoroutine;

        private bool shouldMoveItemIntoViewPort;

        protected override void Awake()
        {
            base.Awake();

            LastSelected = selectedIndex;
            if (LastSelected < 0) LastSelected = 0;

            highlightRT = highlight.GetComponent<RectTransform>();
            scrollRect = GetComponent<ScrollRect>();

            shouldMoveItemIntoViewPort = false;

            focusedOnLabeledSelectables = true;
        }

        public override void DeselectAll()
        {
            base.DeselectAll();

            highlight.gameObject.SetActive(false);

            if (deselectAllCoroutine != null)
            {
                CoroutineProxy.Stop(deselectAllCoroutine);
                deselectAllCoroutine = null;
            }

            deselectAllCoroutine = CoroutineProxy.Start(DeselectAllCoroutine());
        }

        public void FocusOnAdditionalSelectableGroup()
        {
            if (additionalSelectableGroup == null) return;

            focusedOnLabeledSelectables = false;
            DeselectAll();
            additionalSelectableGroup.Select(0);
        }

        public void FocusOnLabeledSelectables()
        {
            if (additionalSelectableGroup == null) return;

            focusedOnLabeledSelectables = true;
            additionalSelectableGroup.DeselectAll();
            Select(LastSelected);
        }

        public override void Confirm()
        {
            if (additionalSelectableGroup == null)
            {
                base.Confirm();
                return;
            }

            if (focusedOnLabeledSelectables)
            {
                base.Confirm();
            }
            else
            {
                additionalSelectableGroup.Confirm();
            }
        }

        private IEnumerator DeselectAllCoroutine()
        {
            float time = 0.2f;
            while (true)
            {
                layoutGroup.SetLayoutVertical();
                time -= Time.deltaTime;
                if (time < 0) break;
                yield return null;
            }

            deselectAllCoroutine = null;
        }

        public void ShouldMoveItemIntoViewPort()
        {
            shouldMoveItemIntoViewPort = true;
        }

        protected override void OnItemSelected(int selection)
        {
            base.OnItemSelected(selection);

            LastSelected = selectedIndex;

            var item = items[selection];
            var itemRT = item.GetComponent<RectTransform>();

            highlight.gameObject.SetActive(true);

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                sequence.Kill();
                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(MoveHighlightToItem(itemRT));

            if (shouldMoveItemIntoViewPort)
            {
                if (scrollCoroutine != null)
                {
                    CoroutineProxy.Stop(scrollCoroutine);
                    scrollCoroutine = null;
                }
                scrollCoroutine = scrollRect.MoveItemIntoViewportSmooth(itemRT, 0.1f, 0.1f);
                shouldMoveItemIntoViewPort = false;
            }
        }

        private IEnumerator MoveHighlightToItem(RectTransform itemRT)
        {
            var startPos = highlight.anchoredPosition;

            sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(0, 1, 0.1f, value =>
            {
                // we calculate targetPos every frame because the item may move
                var itemPos = itemRT.anchoredPosition;
                var worldPos = itemRT.parent.TransformPoint(itemPos);
                var targetPos = highlight.parent.InverseTransformPoint(worldPos);

                highlightRT.anchoredPosition = Vector2.Lerp(startPos, targetPos, value);

                layoutGroup.SetLayoutVertical();
            }));

            yield return sequence.WaitForCompletion();

            coroutine = null;
            sequence = null;

            scrollCoroutine = null;
        }

        public void Clear()
        {
            foreach (Transform child in contentGroup)
            {
                Destroy(child.gameObject);
            }

            items.Clear();
        }

        public LabeledListSelectableItem Add(string label, string text)
        {
            var item = Instantiate(itemPrefab, contentGroup);
            item.gameObject.name = text;
            item.Label = label;
            item.Text = text;
            item.onSelected.AddListener(() =>
            {
                if (additionalSelectableGroup == null) return;
                additionalSelectableGroup.DeselectAll();
                focusedOnLabeledSelectables = true;
            });
            items.Add(item);
            return item;
        }

        public void Initialize()
        {
            selectedIndex = 0;
            base.Awake();
        }
    }
}