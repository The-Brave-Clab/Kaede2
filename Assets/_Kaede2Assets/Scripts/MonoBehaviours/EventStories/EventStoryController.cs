using System;
using System.Collections;
using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Kaede2
{
    public class EventStoryController : MonoBehaviour
    {
        [SerializeField]
        private GameObject sceneRoot;

        [SerializeField]
        private RandomizeScatterImages randomizeScatterImages;

        [SerializeField]
        private StoryCategorySelectable[] storyCategorySelectables;

        [SerializeField]
        private LabeledListSelectableGroup episodeSelectableGroup;

        [SerializeField]
        private LabeledListSelectableGroup storySelectableGroup;

        [SerializeField]
        private Canvas randomizedImageBackgroundCanvas;

        [SerializeField]
        private Canvas selectionCanvas;

        [SerializeField]
        private InterfaceTitle birthdayTitle;

        [SerializeField]
        private InterfaceTitle eventTitle;

        [SerializeField]
        private StorySelectionBackground storySelectionBackground;

        [SerializeField]
        private EpisodeTitle episodeTitle;

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

            selectionCanvas.gameObject.SetActive(false);
            episodeSelectableGroup.transform.parent.gameObject.SetActive(true);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);

            yield return SceneTransition.Fade(0);
        }

        public void EnterEpisodeSelection(bool isBirthday)
        {
            CoroutineProxy.Start(EnterEpisodeSelectionCoroutine(isBirthday ? MasterScenarioInfo.Kind.Birthday : MasterScenarioInfo.Kind.Event));
        }

        private IEnumerator EnterEpisodeSelectionCoroutine(MasterScenarioInfo.Kind kind)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(true);

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

            episodeSelectableGroup.Clear();
            foreach (var info in scenarioChapterInfos)
            {
                string illust = GetCardIllustFromScenarioInfo(info);

                var item = episodeSelectableGroup.Add(info.EpisodeNumber, info.EpisodeName);
                item.onSelected.AddListener(() =>
                {
                    storySelectionBackground.Set(illust);
                });
                item.onConfirmed.AddListener(() =>
                {
                    EnterStorySelection(info);
                });
            }
            episodeSelectableGroup.Initialize();

            yield return null;
            yield return null;

            yield return SceneTransition.Fade(0);
        }

        private void EnterStorySelection(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            CoroutineProxy.Start(EnterStorySelectionCoroutine(scenarioInfo));
        }

        private IEnumerator EnterStorySelectionCoroutine(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            yield return SceneTransition.Fade(1);

            episodeTitle.Label = scenarioInfo.EpisodeNumber;
            episodeTitle.Text = scenarioInfo.EpisodeName;

            episodeSelectableGroup.transform.parent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(true);

            var storyInfos = MasterScenarioInfo.Instance.scenarioInfo
                .Where(si => si.EpisodeId == scenarioInfo.EpisodeId)
                .OrderBy(si => si.StoryId)
                .ToList();

            storySelectableGroup.Clear();
            foreach (var info in storyInfos)
            {
                var item = storySelectableGroup.Add(info.Label, info.StoryName);
                var selectionItem = item.GetComponent<StorySelectionItem>();
                selectionItem.Unread = !SaveData.ReadScenarioNames.Contains(info.ScenarioName);
                selectionItem.FavoriteIcon.gameObject.SetActive(!selectionItem.Unread);
                selectionItem.FavoriteIcon.OnClicked = () =>
                {
                    if (SaveData.IsScenarioFavorite(info))
                        SaveData.RemoveFavoriteScenario(info);
                    else
                        SaveData.AddFavoriteScenario(info);
                };
                selectionItem.FavoriteIcon.IsFavorite = () => SaveData.IsScenarioFavorite(info);
                item.onSelected.AddListener(() =>
                {
                    if (!selectionItem.Unread)
                        selectionItem.FavoriteIcon.gameObject.SetActive(true);
                });
                item.onDeselected.AddListener(() =>
                {
                    if (!selectionItem.Unread)
                        selectionItem.FavoriteIcon.gameObject.SetActive(false);
                });
                item.onConfirmed.AddListener(() =>
                {
                    CoroutineProxy.Start(EnterScenario(info, selectionItem));
                });
            }
            storySelectableGroup.Initialize();

            yield return null;
            yield return null;

            yield return SceneTransition.Fade(0);
        }

        private IEnumerator EnterScenario(MasterScenarioInfo.ScenarioInfo scenario, StorySelectionItem item)
        {
            yield return SceneTransition.Fade(1);

            sceneRoot.SetActive(false);
            yield return PlayerScenarioModule.Play(
                scenario.ScenarioName,
                LocalizationManager.AllLocales.First(),
                LoadSceneMode.Additive,
                null,
                BackToStorySelection
            );

            SaveData.AddReadScenario(scenario);
            item.Unread = false;
        }

        private void BackToStorySelection()
        {
            CoroutineProxy.Start(BackToStorySelectionCoroutine());
        }

        private IEnumerator BackToStorySelectionCoroutine()
        {
            yield return PlayerScenarioModule.Unload();

            sceneRoot.SetActive(true);

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
