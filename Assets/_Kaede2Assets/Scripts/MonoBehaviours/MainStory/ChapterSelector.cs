using System.Collections.Generic;
using Kaede2.UI.Framework;
using UnityEngine;

namespace Kaede2
{
    public class ChapterSelector : MonoBehaviour
    {
        [SerializeField]
        private SelectableItem selectableItem;

        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private List<MainStoryChapter> chapters;

        private int currentChapterIndex;
        private StorySelectionSceneController controller;

        private void Awake()
        {
            selectableItem.onSelected.AddListener(() => selectedOutline.SetActive(true));
            selectableItem.onDeselected.AddListener(() => selectedOutline.SetActive(false));

            selectedOutline.SetActive(selectableItem.selected);

            currentChapterIndex = 0;
        }

        public void SetSceneController(StorySelectionSceneController sceneController)
        {
            controller = sceneController;
        }

        public void OnConfirm()
        {
            var chapter = chapters[currentChapterIndex];
            controller.EnterEpisodeSelection(chapter);
        }
    }
}