using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class EventStoryController : StorySelectionSceneController
    {
        [SerializeField]
        private RandomizeScatterImages randomizeScatterImages;

        [SerializeField]
        private StoryCategorySelectable[] storyCategorySelectables;

        [SerializeField]
        private Canvas randomizedImageBackgroundCanvas;

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

            InitialSetup();

            yield return SceneTransition.Fade(0);
        }

        public void EnterEpisodeSelection(bool isBirthday)
        {
            EnterEpisodeSelection(new EventStoryProvider(isBirthday));
        }

        protected override void OnEnterEpisodeSelection(MasterScenarioInfo.IProvider provider)
        {
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);

            if (provider is not EventStoryProvider eventStoryProvider)
                return;

            if (eventStoryProvider.IsBirthday)
            {
                birthdayTitle.gameObject.SetActive(true);
                eventTitle.gameObject.SetActive(false);
            }
            else
            {
                birthdayTitle.gameObject.SetActive(false);
                eventTitle.gameObject.SetActive(true);
            }
        }

        protected override void OnEpisodeItemSelected(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            string illust = GetCardIllustFromScenarioInfo(scenarioInfo);
            storySelectionBackground.Set(illust);
        }

        private string GetCardIllustFromScenarioInfo(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            string result = "";
            var bgInfo = MasterEventEpisodeBg.Instance.Data
                .FirstOrDefault(bg => bg.EpisodeId == scenarioInfo.EpisodeId);
            if (!string.IsNullOrEmpty(bgInfo?.EpisodeBg))
            {
                result = albumExtraInfo.list.FirstOrDefault(ei => ei.replaceEpisodeBackground == bgInfo.EpisodeBg).name;
                return result;
            }

            var storyImageData = MasterEventStoryImageData.Instance.Data
                .FirstOrDefault(si => si.EpisodeId == scenarioInfo.EpisodeId);
            if (!string.IsNullOrEmpty(storyImageData?.FileName))
            {
                result = albumExtraInfo.list.FirstOrDefault(ei => ei.replaceStoryImage == storyImageData.FileName).name;
                return result;
            }

            return result;
        }

        private class EventStoryProvider : MasterScenarioInfo.IProvider
        {
            public bool IsBirthday { get; }

            public EventStoryProvider(bool isBirthday)
            {
                IsBirthday = isBirthday;
            }

            public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
            {
                var kindId = IsBirthday ? MasterScenarioInfo.Kind.Birthday : MasterScenarioInfo.Kind.Event;
                return MasterScenarioInfo.Instance.Data
                    .Where(si => si.KindId == kindId);
            }
        }
    }

}
