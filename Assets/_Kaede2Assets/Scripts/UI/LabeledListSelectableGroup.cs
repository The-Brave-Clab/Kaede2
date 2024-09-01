using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Kaede2.ScriptableObjects;
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

        public void Initialize(Func<MasterScenarioInfo.ScenarioInfo, bool> scenarioInfoFilter)
        {
            foreach (Transform child in contentGroup)
            {
                Destroy(child.gameObject);
            }

            var scenarioInfos = MasterScenarioInfo.Instance.scenarioInfo
                .Where(scenarioInfoFilter)
                .ToList();

            var scenarioChapterInfos = scenarioInfos
                .OrderBy(si => si.ChapterId)
                .ThenBy(si => si.EpisodeId)
                .GroupBy(scenarioInfo => scenarioInfo.EpisodeId)
                .Select(group => group.First())
                .ToList();

            foreach (var info in scenarioChapterInfos)
            {
                var item = Instantiate(itemPrefab, contentGroup);
                item.gameObject.name = info.EpisodeName;
                item.Label = info.EpisodeNumber;
                item.Text = info.EpisodeName;
                items.Add(item);
            }

            selectedIndex = 0;
            base.Awake();
        }
    }
}