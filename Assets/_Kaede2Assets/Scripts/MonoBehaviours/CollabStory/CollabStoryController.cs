using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public class CollabStoryController : StorySelectionSceneController
    {
        [SerializeField]
        private RandomizeScatterImages randomizeScatterImages;

        [SerializeField]
        private StoryCategorySelectable[] storyCategorySelectables;

        [SerializeField]
        private Canvas randomizedImageBackgroundCanvas;

        [SerializeField]
        private StorySelectionBackground storySelectionBackground;

        [SerializeField]
        private AlbumExtraInfo albumExtraInfo;

        [SerializeField]
        private CollabContentController collabContent;

        [SerializeField]
        private CollabCharacterSelectionController characterSelection;

        [SerializeField]
        private CollabCharacterVoiceController characterVoiceController;

        [SerializeField]
        private SelectableGroup selectableGroup;

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
            InputManager.InputAction.CollabStory.Enable();

            InputManager.InputAction.CollabStory.Confirm.performed += Confirm;
            InputManager.InputAction.CollabStory.Cancel.performed += BackToMainScene;
            InputManager.InputAction.CollabStory.Up.performed += NavigateUp;
            InputManager.InputAction.CollabStory.Down.performed += NavigateDown;
            InputManager.InputAction.CollabStory.Left.performed += NavigateLeft;
            InputManager.InputAction.CollabStory.Right.performed += NavigateRight;
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.CollabStory.Confirm.performed -= Confirm;
            InputManager.InputAction.CollabStory.Cancel.performed -= BackToMainScene;
            InputManager.InputAction.CollabStory.Up.performed -= NavigateUp;
            InputManager.InputAction.CollabStory.Down.performed -= NavigateDown;
            InputManager.InputAction.CollabStory.Left.performed -= NavigateLeft;
            InputManager.InputAction.CollabStory.Right.performed -= NavigateRight;

            InputManager.InputAction.CollabStory.Disable();
        }

        public void EnterCollabContent(CollabImageProvider collab)
        {
            CoroutineProxy.Start(EnterCollabContentCoroutine(collab));
        }

        private IEnumerator EnterCollabContentCoroutine(CollabImageProvider collab)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            collabContent.gameObject.SetActive(true);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(false);
            characterVoiceController.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(true);

            yield return collabContent.Initialize(collab);

            yield return SceneTransition.Fade(0);
        }

        public void ExitCollabContent()
        {
            CoroutineProxy.Start(ExitCollabContentCoroutine());
        }

        private IEnumerator ExitCollabContentCoroutine()
        {
            yield return SceneTransition.Fade(1);

            collabContent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(false);
            characterVoiceController.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(false);
            gameObject.SetActive(true);
            randomizedImageBackgroundCanvas.gameObject.SetActive(true);

            yield return SceneTransition.Fade(0);
        }

        public void EnterStorySelection(MasterCollabInfo.CollabType collabType, bool selfIntro)
        {
            var provider = new CollabStoryProvider(collabType, selfIntro);
            EnterStorySelection(provider, provider.Label, provider.Title);
        }

        protected override void OnEnterStorySelection(MasterScenarioInfo.IProvider provider)
        {
            collabContent.gameObject.SetActive(false);
        }

        protected override void OnExitStorySelection()
        {
            collabContent.gameObject.SetActive(true);
        }

        public void EnterCharacterVoiceCharacterSelection(ContentSubProvider provider)
        {
            CoroutineProxy.Start(EnterCharacterVoiceCharacterSelectionCoroutine(provider));
        }

        private IEnumerator EnterCharacterVoiceCharacterSelectionCoroutine(ContentSubProvider provider)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            collabContent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(true);
            characterVoiceController.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(true);

            yield return characterSelection.Initialize(provider.Provider);

            yield return SceneTransition.Fade(0);
        }

        public void EnterCharacterVoice(CollabCharacterSelectionImageProvider provider)
        {
            CoroutineProxy.Start(EnterCharacterVoiceCoroutine(provider.CharacterId));
        }

        private IEnumerator EnterCharacterVoiceCoroutine(CharacterId characterId)
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            collabContent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(false);
            characterVoiceController.gameObject.SetActive(true);
            selectionCanvas.gameObject.SetActive(true);

            yield return characterVoiceController.Initialize(characterId);

            yield return SceneTransition.Fade(0);
        }

        public void ExitCharacterVoice()
        {
            CoroutineProxy.Start(ExitCharacterVoiceCoroutine());
        }

        private IEnumerator ExitCharacterVoiceCoroutine()
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            collabContent.gameObject.SetActive(false);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(true);
            characterVoiceController.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(true);

            yield return SceneTransition.Fade(0);
        }

        public void ExitCharacterVoiceCharacterSelection()
        {
            CoroutineProxy.Start(ExitCharacterVoiceCharacterSelectionCoroutine());
        }

        private IEnumerator ExitCharacterVoiceCharacterSelectionCoroutine()
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            randomizedImageBackgroundCanvas.gameObject.SetActive(false);
            collabContent.gameObject.SetActive(true);
            storySelectableGroup.transform.parent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(false);
            characterVoiceController.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(true);

            yield return SceneTransition.Fade(0);
        }

        protected override void InitialSetup()
        {
            base.InitialSetup();
            collabContent.gameObject.SetActive(false);
            characterSelection.gameObject.SetActive(false);
            characterVoiceController.gameObject.SetActive(false);
            selectionCanvas.gameObject.SetActive(false);
        }

        private void Confirm(InputAction.CallbackContext obj)
        {
            selectableGroup.SelectedItem.Confirm();
        }

        private void BackToMainScene(InputAction.CallbackContext obj)
        {
            BackToMainScene();
        }

        private void NavigateUp(InputAction.CallbackContext obj)
        {
            if (selectableGroup.SelectedIndex == storyCategorySelectables.Length)
                selectableGroup.Select(storyCategorySelectables.FirstOrDefault(s => s.Activated));
            else
                selectableGroup.Previous();
        }

        private void NavigateDown(InputAction.CallbackContext obj)
        {
            if (storyCategorySelectables.Contains(selectableGroup.SelectedItem))
                selectableGroup.Select(storyCategorySelectables.Length);
            else
                selectableGroup.Next();
        }

        private void NavigateLeft(InputAction.CallbackContext obj)
        {
            StoryCategorySelectable scs = selectableGroup.SelectedItem as StoryCategorySelectable;
            if (scs == null) return;
            if (!storyCategorySelectables.Contains(scs)) return;
            selectableGroup.Select(Mathf.Clamp(Array.IndexOf(storyCategorySelectables, scs) - 1, 0, storyCategorySelectables.Length - 1));
        }

        private void NavigateRight(InputAction.CallbackContext obj)
        {
            StoryCategorySelectable scs = selectableGroup.SelectedItem as StoryCategorySelectable;
            if (scs == null) return;
            if (!storyCategorySelectables.Contains(scs)) return;
            selectableGroup.Select(Mathf.Clamp(Array.IndexOf(storyCategorySelectables, scs) + 1, 0, storyCategorySelectables.Length - 1));
        }

        private class CollabStoryProvider : MasterScenarioInfo.IProvider
        {
            private readonly MasterCollabInfo.CollabType collabType;
            private readonly MasterCollabInfo.CollabInfo collabInfo;
            private readonly bool selfIntro;

            public string Label => selfIntro ? "自己紹介" : "コラボ"; // just hardcode it, it's fine before we add localization to this part
            public string Title => collabInfo?.CollabName;

            public CollabStoryProvider(MasterCollabInfo.CollabType type, bool selfIntro)
            {
                collabType = type;
                collabInfo = MasterCollabInfo.Instance.Data
                    .FirstOrDefault(ci => ci.CollabType == collabType);
                this.selfIntro = selfIntro;
            }

            public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
            {
                if (collabInfo != null)
                    return MasterScenarioInfo.Instance.Data
                        .Where(si => si.EpisodeId == (selfIntro ? collabInfo.SelfIntroEpisodeId : collabInfo.StoryEpisodeId));

                // this should never happen but just in case
                this.LogError($"Collab info not found for {collabType:G}");
                return Array.Empty<MasterScenarioInfo.ScenarioInfo>();
            }
        }
    }

}
