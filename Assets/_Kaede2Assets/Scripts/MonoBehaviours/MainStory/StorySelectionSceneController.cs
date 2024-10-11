using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using CommonUtils = Kaede2.Utils.CommonUtils;

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

        protected virtual void OnExitEpisodeSelection()
        {
            
        }

        protected virtual void OnEpisodeItemSelected(MasterScenarioInfo.ScenarioInfo scenarioInfo)
        {
            
        }

        protected virtual void OnEnterStorySelection(MasterScenarioInfo.IProvider provider)
        {
            
        }

        protected virtual void OnExitStorySelection()
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
            {
                episodeSelectableGroup.transform.parent.gameObject.SetActive(false);
            }
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
                episodeSelectableGroup.transform.parent.gameObject.SetActive(true);

                yield return null;
                yield return null;
            }

            yield return SceneTransition.Fade(0);

            if (episodeSelectableGroup != null)
                EnableEpisodeSelectionControl();
        }

        public void ExitEpisodeSelection()
        {
            CoroutineProxy.Start(ExitEpisodeSelectionCoroutine());
        }

        private IEnumerator ExitEpisodeSelectionCoroutine()
        {
            yield return SceneTransition.Fade(1);

            if (episodeSelectableGroup != null)
            {
                episodeSelectableGroup.Clear();
                episodeSelectableGroup.transform.parent.gameObject.SetActive(false);
                DisableEpisodeSelectionControl();
            }
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            OnExitEpisodeSelection();
            selectionCanvas.gameObject.SetActive(false);
            gameObject.SetActive(true);

            yield return SceneTransition.Fade(0);
        }

        public void EnterStorySelection(MasterScenarioInfo.IProvider provider, string titleLabel, string titleText)
        {
            CoroutineProxy.Start(EnterStorySelectionCoroutine(provider, titleLabel, titleText));
        }

        private IEnumerator EnterStorySelectionCoroutine(MasterScenarioInfo.IProvider provider, string titleLabel, string titleText)
        {
            if (episodeSelectableGroup != null)
                DisableEpisodeSelectionControl();

            yield return SceneTransition.Fade(1);

            episodeTitle.Label = titleLabel;
            episodeTitle.Text = titleText;

            if (episodeSelectableGroup != null)
                episodeSelectableGroup.transform.parent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
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

            storySelectableGroup.transform.parent.gameObject.SetActive(true);

            yield return null;
            yield return null;

            yield return SceneTransition.Fade(0);

            EnableStorySelectionControl();
        }

        public void ExitStorySelection()
        {
            CoroutineProxy.Start(ExitStorySelectionCoroutine());
        }

        private IEnumerator ExitStorySelectionCoroutine()
        {
            DisableStorySelectionControl();
    
            yield return SceneTransition.Fade(1);

            if (episodeSelectableGroup != null)
                episodeSelectableGroup.transform.parent.gameObject.SetActive(true);
            storySelectableGroup.Clear();
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            OnExitStorySelection();
            selectionCanvas.gameObject.SetActive(true);
            gameObject.SetActive(false);

            yield return SceneTransition.Fade(0);

            if (episodeSelectableGroup != null)
                EnableEpisodeSelectionControl();
        }

        protected virtual bool AdditionalStoryFilter(MasterScenarioInfo.ScenarioInfo scenario)
        {
            return true;
        }

        public IEnumerator EnterScenario(MasterScenarioInfo.ScenarioInfo scenario, CultureInfo language)
        {
            DisableStorySelectionControl();
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

            EnableStorySelectionControl();
        }

        private void EnableEpisodeSelectionControl()
        {
            InputManager.InputAction.StoryEpisodeSelection.Enable();

            InputManager.InputAction.StoryEpisodeSelection.Up.performed += PreviousEpisode;
            InputManager.InputAction.StoryEpisodeSelection.Down.performed += NextEpisode;
            InputManager.InputAction.StoryEpisodeSelection.Confirm.performed += ConfirmEpisode;
            InputManager.InputAction.StoryEpisodeSelection.Cancel.performed += ExitEpisode;
        }

        private void DisableEpisodeSelectionControl()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.StoryEpisodeSelection.Up.performed -= PreviousEpisode;
            InputManager.InputAction.StoryEpisodeSelection.Down.performed -= NextEpisode;
            InputManager.InputAction.StoryEpisodeSelection.Confirm.performed -= ConfirmEpisode;
            InputManager.InputAction.StoryEpisodeSelection.Cancel.performed -= ExitEpisode;

            InputManager.InputAction.StoryEpisodeSelection.Disable();
        }

        private void PreviousEpisode(InputAction.CallbackContext obj)
        {
            if (episodeSelectableGroup == null) return;
            episodeSelectableGroup.Previous();
        }

        private void NextEpisode(InputAction.CallbackContext obj)
        {
            if (episodeSelectableGroup == null) return;
            episodeSelectableGroup.Next();
        }

        private void ConfirmEpisode(InputAction.CallbackContext obj)
        {
            if (episodeSelectableGroup == null) return;
            episodeSelectableGroup.Confirm();
        }

        private void ExitEpisode(InputAction.CallbackContext obj)
        {
            ExitEpisodeSelection();
        }

        private void EnableStorySelectionControl()
        {
            InputManager.InputAction.StorySelection.Enable();

            InputManager.InputAction.StorySelection.Up.performed += PreviousStory;
            InputManager.InputAction.StorySelection.Down.performed += NextStory;
            InputManager.InputAction.StorySelection.Confirm.performed += ConfirmStory;
            InputManager.InputAction.StorySelection.Cancel.performed += ExitStory;
        }

        private void DisableStorySelectionControl()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.StorySelection.Up.performed -= PreviousStory;
            InputManager.InputAction.StorySelection.Down.performed -= NextStory;
            InputManager.InputAction.StorySelection.Confirm.performed -= ConfirmStory;
            InputManager.InputAction.StorySelection.Cancel.performed -= ExitStory;

            InputManager.InputAction.StorySelection.Disable();
        }

        private void PreviousStory(InputAction.CallbackContext obj)
        {
            storySelectableGroup.Previous();
        }

        private void NextStory(InputAction.CallbackContext obj)
        {
            storySelectableGroup.Next();
        }

        private void ConfirmStory(InputAction.CallbackContext obj)
        {
            storySelectableGroup.Confirm();
        }

        private void ExitStory(InputAction.CallbackContext obj)
        {
            ExitStorySelection();
        }

        public void BackToMainScene()
        {
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        public void GoToMainStory()
        {
            CommonUtils.LoadNextScene("MainStoryScene", LoadSceneMode.Single);
        }

        public void GoToEventStory()
        {
            CommonUtils.LoadNextScene("EventStoryScene", LoadSceneMode.Single);
        }

        public void GoToCollabStory()
        {
            CommonUtils.LoadNextScene("CollabStoryScene", LoadSceneMode.Single);
        }

        public void GoToFavoriteStory()
        {
            CommonUtils.LoadNextScene("FavoriteStoryScene", LoadSceneMode.Single);
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