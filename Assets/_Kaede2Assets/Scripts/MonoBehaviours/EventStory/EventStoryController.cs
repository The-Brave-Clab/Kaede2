using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public class EventStoryController : StorySelectionSceneController, Kaede2InputAction.IEventStoryActions
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

        [SerializeField]
        private SelectableGroup selectableGroup;

        [SerializeField]
        private StoryCategorySelectable eventStoryItem;

        [SerializeField]
        private StoryCategorySelectable birthdayStoryItem;

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

        private void OnEnable()
        {
            InputManager.InputAction.EventStory.Enable();
            InputManager.InputAction.EventStory.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.EventStory.RemoveCallbacks(this);
            InputManager.InputAction.EventStory.Disable();
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

        protected override void OnExitEpisodeSelection()
        {
            randomizedImageBackgroundCanvas.gameObject.SetActive(true);
            birthdayTitle.gameObject.SetActive(false);
            eventTitle.gameObject.SetActive(false);
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

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectableGroup.SelectedIndex is 0 or 1) return; // when the first or second item is selected
            if (selectableGroup.SelectedIndex != 2) // common up
            {
                selectableGroup.Previous();
                return;
            }

            selectableGroup.Select(eventStoryItem.Activated ? eventStoryItem : birthdayStoryItem);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectableGroup.SelectedIndex is 0 or 1)
            {
                selectableGroup.Select(2);
                return;
            }

            selectableGroup.Next();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectableGroup.SelectedIndex != 1) return;
            selectableGroup.Select(0);
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectableGroup.SelectedIndex != 0) return;
            selectableGroup.Select(1);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            selectableGroup.Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            BackToMainScene();
        }
    }

}
