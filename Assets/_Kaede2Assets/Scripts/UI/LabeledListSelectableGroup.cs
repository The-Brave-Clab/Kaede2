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

        private RectTransform highlightRT;

        private ScrollRect scrollRect;

        private Coroutine coroutine;
        private Sequence sequence;

        private Coroutine scrollCoroutine;

        private bool shouldMoveItemIntoViewPort;

        protected override void Awake()
        {
            base.Awake();

            highlightRT = highlight.GetComponent<RectTransform>();
            scrollRect = GetComponent<ScrollRect>();

            shouldMoveItemIntoViewPort = false;
        }

        public void ShouldMoveItemIntoViewPort()
        {
            shouldMoveItemIntoViewPort = true;
        }

        protected override void OnItemSelected(int selection)
        {
            base.OnItemSelected(selection);

            var item = items[selection];
            var itemRT = item.GetComponent<RectTransform>();

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