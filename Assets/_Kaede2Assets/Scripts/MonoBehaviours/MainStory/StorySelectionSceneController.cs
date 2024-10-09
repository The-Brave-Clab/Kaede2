using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public abstract class StorySelectionSceneController : MonoBehaviour
    {
        [SerializeField]
        protected GameObject sceneRoot;

        [SerializeField]
        protected Canvas selectionCanvas;

        [SerializeField]
        protected LabeledListSelectableGroup episodeSelectableGroup;

        [SerializeField]
        protected LabeledListSelectableGroup storySelectableGroup;

        [SerializeField]
        protected EpisodeTitle episodeTitle;

        [SerializeField]
        private CharacterWindow characterWindow;

        protected List<StorySelectionItem> storySelectionItems;

        public CharacterWindow CharacterWindow => characterWindow;

        protected virtual void Awake()
        {
            storySelectionItems = new List<StorySelectionItem>();
        }

        protected virtual void InitialSetup()
        {
            selectionCanvas.gameObject.SetActive(false);
            if (episodeSelectableGroup != null)
                episodeSelectableGroup.transform.parent.gameObject.SetActive(true);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
        }

        protected virtual void OnEnterEpisodeSelection(MasterScenarioInfo.IProvider provider)
        {
            
        }

        protected virtual void OnEpisodeItemSelected(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            
        }

        protected virtual void OnEnterStorySelection(MasterScenarioInfo.IProvider provider)
        {
            
        }

        public void EnterEpisodeSelection(MasterScenarioInfo.IProvider provider)
        {
            CoroutineProxy.Start(EnterEpisodeSelectionCoroutine(provider));
        }

        private IEnumerator EnterEpisodeSelectionCoroutine(MasterScenarioInfo.IProvider provider)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            if (episodeSelectableGroup != null)
                episodeSelectableGroup.transform.parent.gameObject.SetActive(true);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            OnEnterEpisodeSelection(provider);
            selectionCanvas.gameObject.SetActive(true);

            if (episodeSelectableGroup != null)
            {
                var scenarioChapterInfos = provider.Provide()
                    .OrderBy(si => si.ChapterId)
                    .ThenBy(si => si.EpisodeId)
                    .GroupBy(si => si.EpisodeId)
                    .Select(group => group.First())
                    .ToList();

                episodeSelectableGroup.Clear();
                foreach (var info in scenarioChapterInfos)
                {
                    var item = episodeSelectableGroup.Add(info.EpisodeNumber, info.EpisodeName);
                    item.onSelected.AddListener(() => { OnEpisodeItemSelected(info); });
                    item.onConfirmed.AddListener(() =>
                    {
                        EnterStorySelection(new SameEpisodeProvider(info), info.EpisodeNumber, info.EpisodeName);
                    });
                }

                episodeSelectableGroup.Initialize();

                yield return null;
                yield return null;
            }

            yield return SceneTransition.Fade(0);
        }

        public void EnterStorySelection(MasterScenarioInfo.IProvider provider, string titleLabel, string titleText)
        {
            CoroutineProxy.Start(EnterStorySelectionCoroutine(provider, titleLabel, titleText));
        }

        private IEnumerator EnterStorySelectionCoroutine(MasterScenarioInfo.IProvider provider, string titleLabel, string titleText)
        {
            yield return SceneTransition.Fade(1);

            episodeTitle.Label = titleLabel;
            episodeTitle.Text = titleText;

            if (episodeSelectableGroup != null)
                episodeSelectableGroup.transform.parent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(true);
            OnEnterStorySelection(provider);
            selectionCanvas.gameObject.SetActive(true);

            var storyInfos = provider.Provide()
                .Where(AdditionalStoryFilter)
                .OrderBy(si => si.StoryId)
                .ToList();

            storySelectableGroup.Clear();
            storySelectionItems.Clear();
            foreach (var info in storyInfos)
            {
                var item = storySelectableGroup.Add(info.Label, info.StoryName);
                var selectionItem = item.GetComponent<StorySelectionItem>();
                selectionItem.Initialize(info, this);
                storySelectionItems.Add(selectionItem);
            }
            storySelectableGroup.Initialize();

            yield return null;
            yield return null;

            yield return SceneTransition.Fade(0);
        }

        protected virtual bool AdditionalStoryFilter(MasterScenarioInfo.ScenarioInfo scenario)
        {
            return true;
        }

        public IEnumerator EnterScenario(MasterScenarioInfo.ScenarioInfo scenario, CultureInfo language)
        {
            yield return SceneTransition.Fade(1);

            sceneRoot.SetActive(false);
            yield return PlayerScenarioModule.Play(
                scenario.ScenarioName,
                language,
                LoadSceneMode.Additive,
                null,
                () =>
                {
                    BackToStorySelection();
                    foreach (var selectionItem in storySelectionItems)
                    {
                        selectionItem.Refresh();
                    }
                }
            );
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

        public class SameEpisodeProvider : MasterScenarioInfo.IProvider
        {
            private MasterScenarioInfo.ScenarioInfo scenarioInfo;

            public SameEpisodeProvider(MasterScenarioInfo.ScenarioInfo info)
            {
                scenarioInfo = info;
            }

            public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
            {
                return MasterScenarioInfo.Instance.Data
                    .Where(si => si.EpisodeId == scenarioInfo.EpisodeId);
            }
        }
    }
}