using System.Collections;
using System.Collections.Generic;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainStorySceneController : StorySelectionSceneController, Kaede2InputAction.IMainStoryActions
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
        }

        private void OnEnable()
        {
            InputManager.InputAction.MainStory.Enable();
            InputManager.InputAction.MainStory.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.MainStory.RemoveCallbacks(this);
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

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectableGroup.Previous())
                AudioManager.ButtonSound();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectableGroup.Next())
                AudioManager.ButtonSound();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (!chapterSelectable.selected) return;
            chapterSelector.SelectPrevious();
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (!chapterSelectable.selected) return;
            chapterSelector.SelectNext();
        }

        public void OnLeftShoulder(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            chapterSelector.SelectPrevious();
        }

        public void OnRightShoulder(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            chapterSelector.SelectNext();
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            AudioManager.ConfirmSound();
            selectableGroup.Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            BackToMainScene();
        }
    }
}