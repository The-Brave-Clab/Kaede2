using System;
using System.Collections;
using System.Collections.Generic;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainStorySceneController : StorySelectionSceneController
    {
        [SerializeField]
        private string backgroundSpriteName;

        [SerializeField]
        private List<Image> backgroundImage;

        [SerializeField]
        private ChapterSelector chapterSelector;

        [SerializeField]
        private InterfaceTitle episodeSelectionTitle;

        [SerializeField]
        private OverlayBlend mainOverlay;

        [SerializeField]
        private OverlayBlend subOverlay;

        [SerializeField]
        private SelectableGroup selectableGroup;

        [SerializeField]
        private SelectableItem chapterSelectable;

        [SerializeField]
        private ArrowButtonWithDecor leftArrow;

        [SerializeField]
        private ArrowButtonWithDecor rightArrow;

        private AsyncOperationHandle<Sprite> backgroundHandle;

        private IEnumerator Start()
        {
            backgroundHandle = ResourceLoader.LoadIllustration(backgroundSpriteName);

            chapterSelector.SetSceneController(this);
            
            yield return backgroundHandle;
            foreach (var image in backgroundImage)
            {
                image.sprite = backgroundHandle.Result;
            }

            InitialSetup();

            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            if (backgroundHandle.IsValid())
                Addressables.Release(backgroundHandle);

            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.MainStory.Cancel.performed -= BackToMainScene;
                InputManager.InputAction.MainStory.Down.performed -= Next;
                InputManager.InputAction.MainStory.Up.performed -= Previous;
                InputManager.InputAction.MainStory.LeftShoulder.performed -= PreviousChapter;
                InputManager.InputAction.MainStory.RightShoulder.performed -= NextChapter;
                InputManager.InputAction.MainStory.Confirm.performed -= Confirm;
    
                InputManager.InputAction.MainStory.Disable();
            }
        }

        private void OnEnable()
        {
            InputManager.InputAction.MainStory.Enable();

            InputManager.InputAction.MainStory.Cancel.performed += BackToMainScene;
            InputManager.InputAction.MainStory.Down.performed += Next;
            InputManager.InputAction.MainStory.Up.performed += Previous;
            InputManager.InputAction.MainStory.LeftShoulder.performed += PreviousChapter;
            InputManager.InputAction.MainStory.RightShoulder.performed += NextChapter;
            InputManager.InputAction.MainStory.Confirm.performed += Confirm;
            chapterSelectable.onSelected.AddListener(ChapterSelectableSelected);
            chapterSelectable.onDeselected.AddListener(ChapterSelectableDeselected);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.MainStory.Cancel.performed -= BackToMainScene;
            InputManager.InputAction.MainStory.Down.performed -= Next;
            InputManager.InputAction.MainStory.Up.performed -= Previous;
            InputManager.InputAction.MainStory.LeftShoulder.performed -= PreviousChapter;
            InputManager.InputAction.MainStory.RightShoulder.performed -= NextChapter;
            InputManager.InputAction.MainStory.Confirm.performed -= Confirm;
            chapterSelectable.onSelected.RemoveListener(ChapterSelectableSelected);
            chapterSelectable.onDeselected.RemoveListener(ChapterSelectableDeselected);

            InputManager.InputAction.MainStory.Disable();
        }

        protected override void OnEnterEpisodeSelection(MasterScenarioInfo.IProvider provider)
        {
            if (provider is not MainStoryChapter chapter)
                return;

            episodeSelectionTitle.Text = chapter.Text.Replace('\n', ' ');
            mainOverlay.gameObject.SetActive(!chapter.IsSub);
            subOverlay.gameObject.SetActive(chapter.IsSub);
        }

        protected override void OnExitEpisodeSelection()
        {
            episodeSelectionTitle.Text = "";
            mainOverlay.gameObject.SetActive(false);
            subOverlay.gameObject.SetActive(false);
        }

        private void BackToMainScene(InputAction.CallbackContext obj)
        {
            BackToMainScene();
        }

        public void BackToMainScene()
        {
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        private void Next(InputAction.CallbackContext obj)
        {
            selectableGroup.Next();
        }

        private void Previous(InputAction.CallbackContext obj)
        {
            selectableGroup.Previous();
        }

        private void NextChapter(InputAction.CallbackContext obj)
        {
            chapterSelector.SelectNext();
        }

        private void PreviousChapter(InputAction.CallbackContext obj)
        {
            chapterSelector.SelectPrevious();
        }

        private void Confirm(InputAction.CallbackContext obj)
        {
            selectableGroup.Confirm();
        }

        private void ChapterSelectableSelected()
        {
            InputManager.InputAction.MainStory.Left.performed += PreviousChapter;
            InputManager.InputAction.MainStory.Right.performed += NextChapter;
        }

        private void ChapterSelectableDeselected()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.MainStory.Left.performed -= PreviousChapter;
            InputManager.InputAction.MainStory.Right.performed -= NextChapter;
        }
    }
}