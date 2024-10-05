using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonEpisodeSelection : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private CartoonChapterPanel chapterPanel;

        [SerializeField]
        private CartoonEpisodeGroup[] episodes;

        [SerializeField]
        private string[] episodeLabelPrefixes;

        [SerializeField]
        private RectTransform selectionFrame;

        private CartoonEpisodeGroup currentSelected;

        private CartoonSceneController sceneController;

        private Image selectionFrameImage;
        private RectTransform selectionFrameParent;
        private Dictionary<CartoonEpisodeGroup, RectTransform> episodeRects;

        private Coroutine coroutine;
        private Sequence sequence;

        private void Awake()
        {
            selectionFrameImage = selectionFrame.GetComponent<Image>();
            selectionFrameParent = selectionFrame.parent as RectTransform;

            episodeRects = new();
            foreach (var episode in episodes)
            {
                episodeRects.Add(episode, episode.transform.GetChild(0) as RectTransform);
            }

            currentSelected = episodes[0];

            OnThemeChange(Theme.Current);
        }

        public IEnumerator Initialize(CartoonSceneController controller, CartoonChapterSelection chapter)
        {
            sceneController = controller;

            var cartoonInfos = MasterCartoonInfo.Instance.Data
                .Where(ci => ci.GroupId == chapter.ChapterInfo.GroupId)
                .OrderBy(ci => ci.No)
                .ToList();

            chapterPanel.Title = chapter.Panel.Title;
            chapterPanel.ChapterNumber = chapter.Panel.ChapterNumber;
            chapterPanel.Thumbnail = chapter.Panel.Thumbnail;

            CoroutineGroup group = new();
            for (int i = 0; i < 4; ++i)
            {
                group.Add(episodes[i].Initialize(this, episodeLabelPrefixes[i], cartoonInfos[i]));
            }
            yield return group.WaitForAll();

            var (pos, size) = GetSelectionFrameRectFromEpisode(currentSelected);
            selectionFrame.anchoredPosition = pos;
            selectionFrame.sizeDelta = size;
        }

        private (Vector2 position, Vector2 size) GetSelectionFrameRectFromEpisode(CartoonEpisodeGroup episode)
        {
            var rect = episodeRects[episode];
            // get world corners
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            // convert to local space
            for (int i = 0; i < 4; ++i)
            {
                corners[i] = selectionFrameParent.InverseTransformPoint(corners[i]);
            }

            Vector2 min = corners[0];
            Vector2 max = corners[2];
            var size = max - min;
            var position = min + size / 2;
            return (position, size + Vector2.one * 10.0f);
        }

        public void Select(CartoonEpisodeGroup episode)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                sequence.Kill();
                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(SelectCoroutine(episode));
        }

        private IEnumerator SelectCoroutine(CartoonEpisodeGroup episode)
        {
            var currentPos = selectionFrame.anchoredPosition;
            var currentSize = selectionFrame.sizeDelta;

            var (newPos, newSize) = GetSelectionFrameRectFromEpisode(episode);

            sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(0, 1, 0.2f, t =>
            {
                selectionFrame.anchoredPosition = Vector2.Lerp(currentPos, newPos, t);
                selectionFrame.sizeDelta = Vector2.Lerp(currentSize, newSize, t);
            }));

            yield return sequence.WaitForCompletion();

            coroutine = null;
            sequence = null;
        }

        public void Confirm(CartoonEpisodeGroup episode)
        {
            sceneController.OnEpisodeSelected(episode.CartoonInfo);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            if (selectionFrameImage == null) return;
            selectionFrameImage.color = theme.CommonButtonColor.NonTransparent().surface;
        }
    }
}