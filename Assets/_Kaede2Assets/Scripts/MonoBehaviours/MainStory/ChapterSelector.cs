using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kaede2.Audio;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2
{
    public class ChapterSelector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private SelectableItem selectableItem;

        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private List<MainStoryChapter> chapters;

        [SerializeField]
        private HorizontalLayoutGroup layout;

        private int currentChapterIndex;
        private StorySelectionSceneController controller;

        private RectTransform rt;
        private RectTransform layoutRT;

        private Coroutine selectCoroutine;
        private Sequence selectSequence;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            layoutRT = layout.GetComponent<RectTransform>();

            selectableItem.onSelected.AddListener(() => selectedOutline.SetActive(true));
            selectableItem.onDeselected.AddListener(() => selectedOutline.SetActive(false));

            selectedOutline.SetActive(selectableItem.selected);

            currentChapterIndex = 0;
            blockSubsequentConfirm = false;
        }

        public void SetSceneController(StorySelectionSceneController sceneController)
        {
            controller = sceneController;
        }

        public void OnConfirm()
        {
            if (blockSubsequentConfirm) return;

            var chapter = chapters[currentChapterIndex];
            controller.EnterEpisodeSelection(chapter);
        }

        public void SelectNext()
        {
            Select(currentChapterIndex + 1);
        }

        public void SelectPrevious()
        {
            Select(currentChapterIndex - 1);
        }

        private void Select(int newIndex)
        {
            var clampedIndex = Mathf.Clamp(newIndex, 0, chapters.Count - 1);
            if (clampedIndex == newIndex)
                AudioManager.ButtonSound();

            ClearCoroutine();

            selectCoroutine = StartCoroutine(SelectCoroutine(clampedIndex));
        }

        private IEnumerator SelectCoroutine(int newIndex)
        {
            currentChapterIndex = newIndex;

            var currentPositionX = layoutRT.anchoredPosition.x;
            var targetPositionX = -1 * (rt.sizeDelta.x + layout.spacing) * newIndex;

            selectSequence = DOTween.Sequence();
            selectSequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
            {
                var position = layoutRT.anchoredPosition;
                position.x = Mathf.Lerp(currentPositionX, targetPositionX, value);
                layoutRT.anchoredPosition = position;
            }));

            yield return selectSequence.WaitForCompletion();

            selectCoroutine = null;
            selectSequence = null;
        }

        private void ClearCoroutine()
        {
            if (selectCoroutine == null) return;

            StopCoroutine(selectCoroutine);
            selectSequence.Kill();
            selectCoroutine = null;
            selectSequence = null;
        }

        private Vector2 dragStartPosition;
        private float dragStartTime;
        private float dragStartLayoutPositionX;
        private bool blockSubsequentConfirm;

        public void OnBeginDrag(PointerEventData eventData)
        {
            ClearCoroutine();

            dragStartPosition = eventData.position;
            dragStartTime = Time.time;
            dragStartLayoutPositionX = layoutRT.anchoredPosition.x;
            blockSubsequentConfirm = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var newDragPosition = eventData.position;
            var deltaX = newDragPosition.x - dragStartPosition.x;

            var newPositionX = dragStartLayoutPositionX + deltaX;
            var position = layoutRT.anchoredPosition;
            position.x = newPositionX;
            layoutRT.anchoredPosition = position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var dragEndPosition = eventData.position;
            var deltaX = dragEndPosition.x - dragStartPosition.x;
            var deltaTime = Time.time - dragStartTime;
            var speed = deltaX / deltaTime;

            var endPositionX = dragStartLayoutPositionX + deltaX;

            bool isSwipe = Mathf.Abs(speed) > 1000;

            var newIndex = Mathf.Clamp(Mathf.RoundToInt(-endPositionX / (rt.sizeDelta.x + layout.spacing)), 0, chapters.Count - 1);
            if (newIndex == currentChapterIndex && isSwipe)
            {
                newIndex = currentChapterIndex + (deltaX < 0 ? 1 : -1);
            }
            Select(newIndex);
            blockSubsequentConfirm = false;
        }
    }
}