using System;
using System.Collections;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class EventStoryController : MonoBehaviour
    {
        [SerializeField]
        private RandomizeScatterImages randomizeScatterImages;

        [SerializeField]
        private StoryCategorySelectable[] storyCategorySelectables;

        [SerializeField]
        private LabeledListSelectableGroup chapterSelectableGroup;

        [SerializeField]
        private Canvas randomizedImageBackgroundCanvas;

        [SerializeField]
        private Canvas chapterSelectionCanvas;

        [SerializeField]
        private InterfaceTitle birthdayTitle;

        [SerializeField]
        private InterfaceTitle eventTitle;

        [SerializeField]
        private StorySelectionBackground storySelectionBackground;

        [SerializeField]
        private AlbumExtraInfo albumExtraInfo;

        private IEnumerator Start()
        {
            IEnumerator WaitForCondition(Func<bool> condition)
            {
                while (!condition())
                    yield return null;
            }

            CoroutineGroup group = new CoroutineGroup();

            group.Add(WaitForCondition(() => randomizeScatterImages.Loaded));
            foreach (var selectable in storyCategorySelectables)
            {
                group.Add(WaitForCondition(() => selectable.Loaded));
            }

            yield return group.WaitForAll();

            chapterSelectionCanvas.gameObject.SetActive(false);

            yield return SceneTransition.Fade(0);
        }

        public void EnterChapterSelection(bool isBirthday)
        {
            CoroutineProxy.Start(EnterChapterSelectionCoroutine(isBirthday ? MasterScenarioInfo.Kind.Birthday : MasterScenarioInfo.Kind.Event));
        }

        private IEnumerator EnterChapterSelectionCoroutine(MasterScenarioInfo.Kind kind)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            chapterSelectionCanvas.gameObject.SetActive(true);

            if (kind == MasterScenarioInfo.Kind.Birthday)
            {
                birthdayTitle.gameObject.SetActive(true);
                eventTitle.gameObject.SetActive(false);
            }
            else if (kind == MasterScenarioInfo.Kind.Event)
            {
                birthdayTitle.gameObject.SetActive(false);
                eventTitle.gameObject.SetActive(true);
            }

            var scenarioInfos = MasterScenarioInfo.Instance.scenarioInfo
                .Where(si => si.KindId == kind)
                .ToList();

            var scenarioChapterInfos = scenarioInfos
                .OrderBy(si => si.ChapterId)
                .ThenBy(si => si.EpisodeId)
                .GroupBy(scenarioInfo => scenarioInfo.EpisodeId)
                .Select(group => group.First())
                .ToList();

            chapterSelectableGroup.Clear();
            foreach (var info in scenarioChapterInfos)
            {
                string illust = GetCardIllustFromScenarioInfo(info);

                var item = chapterSelectableGroup.Add(info.EpisodeNumber, info.EpisodeName);
                item.onSelected.AddListener(() =>
                {
                    storySelectionBackground.Set(illust);
                });
            }
            chapterSelectableGroup.Initialize();

            yield return null;
            yield return null;

            yield return SceneTransition.Fade(0);
        }

        private string GetCardIllustFromScenarioInfo(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            string result = "";
            var bgInfo = MasterEventEpisodeBg.Instance.eventEpisodeBgs
                .FirstOrDefault(bg => bg.EpisodeId == scenarioInfo.EpisodeId);
            if (!string.IsNullOrEmpty(bgInfo?.EpisodeBg))
            {
                result = albumExtraInfo.list.FirstOrDefault(ei => ei.replaceEpisodeBackground == bgInfo.EpisodeBg).name;
                return result;
            }

            var storyImageData = MasterEventStoryImageData.Instance.eventStoryImages
                .FirstOrDefault(si => si.EpisodeId == scenarioInfo.EpisodeId);
            if (!string.IsNullOrEmpty(storyImageData?.FileName))
            {
                result = albumExtraInfo.list.FirstOrDefault(ei => ei.replaceStoryImage == storyImageData.FileName).name;
                return result;
            }

            return result;
        }
    }

}
