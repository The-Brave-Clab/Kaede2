using System.Collections;
using DG.Tweening;
using Kaede2.UI.Framework;
using UnityEngine;

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

        private RectTransform highlightRT;

        private Coroutine coroutine;
        private Sequence sequence;

        protected override void Awake()
        {
            base.Awake();

            highlightRT = highlight.GetComponent<RectTransform>();
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
            }));

            yield return sequence.WaitForCompletion();

            coroutine = null;
            sequence = null;
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