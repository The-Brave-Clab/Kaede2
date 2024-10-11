using System;
using System.Collections;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabContentController : StoryCategorySelectableGroup
    {
        [SerializeField]
        private InterfaceTitle interfaceTitle;

        [SerializeField]
        private CollabStoryController storyController;

        [SerializeField]
        private Image background;

        [SerializeField]
        private ContentSubProvider story;

        [SerializeField]
        private ContentSubProvider selfIntro;

        [SerializeField]
        private ContentSubProvider characterVoice;

        [SerializeField]
        private TextMeshProUGUI storyTitle;

        [SerializeField]
        private TextMeshProUGUI storyTitleOutline;

        private void OnEnable()
        {
            InputManager.InputAction.CollabStoryContent.Enable();

            InputManager.InputAction.CollabStoryContent.Confirm.performed += Confirm;
            InputManager.InputAction.CollabStoryContent.Cancel.performed += BackToStoryController;
            InputManager.InputAction.CollabStoryContent.Up.performed += NavigateUp;
            InputManager.InputAction.CollabStoryContent.Down.performed += NavigateDown;
            InputManager.InputAction.CollabStoryContent.Left.performed += NavigateLeft;
            InputManager.InputAction.CollabStoryContent.Right.performed += NavigateRight;
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.CollabStoryContent.Confirm.performed -= Confirm;
            InputManager.InputAction.CollabStoryContent.Cancel.performed -= BackToStoryController;
            InputManager.InputAction.CollabStoryContent.Up.performed -= NavigateUp;
            InputManager.InputAction.CollabStoryContent.Down.performed -= NavigateDown;
            InputManager.InputAction.CollabStoryContent.Left.performed -= NavigateLeft;
            InputManager.InputAction.CollabStoryContent.Right.performed -= NavigateRight;

            InputManager.InputAction.CollabStoryContent.Disable();
        }

        private void Confirm(InputAction.CallbackContext obj)
        {
            Confirm();
        }

        private void BackToStoryController(InputAction.CallbackContext obj)
        {
            storyController.ExitCollabContent();
        }

        private void NavigateUp(InputAction.CallbackContext obj)
        {
            if (selectedIndex % 2 != 1) return;
            var targetIndex = selectedIndex - 1;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex - 1);
        }

        private void NavigateDown(InputAction.CallbackContext obj)
        {
            if (selectedIndex % 2 != 0) return;
            var targetIndex = selectedIndex + 1;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex + 1);
        }

        private void NavigateLeft(InputAction.CallbackContext obj)
        {
            if (selectedIndex < 2) return;
            var targetIndex = selectedIndex - 2;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex - 2);
        }

        private void NavigateRight(InputAction.CallbackContext obj)
        {
            if (selectedIndex >= 2) return;
            var targetIndex = selectedIndex + 2;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex + 2);
        }

        public IEnumerator Initialize(CollabImageProvider provider)
        {
            interfaceTitle.Text = provider.GetComponent<StoryCategorySelectable>().Text;

            CoroutineGroup group = new();
            group.Add(provider.LoadBackground(bg => background.sprite = bg));
            group.Add(provider.LoadStory(s => story.Initialize(provider, s)));
            group.Add(provider.LoadSelfIntro(s => selfIntro.Initialize(provider, s)));
            group.Add(provider.LoadCharacterVoice(s => characterVoice.Initialize(provider, s)));
            yield return group.WaitForAll();

            group = new();
            group.Add(story.Selectable.Refresh());
            group.Add(selfIntro.Selectable.Refresh());
            group.Add(characterVoice.Selectable.Refresh());
            yield return group.WaitForAll();

            // release the spyce collab doesn't have self introduction
            selfIntro.gameObject.SetActive(provider.CollabType != MasterCollabInfo.CollabType.RELEASE_THE_SPYCE);

            // set story title
            var collabInfo = MasterCollabInfo.Instance.Data.FirstOrDefault(ci => ci.CollabType == provider.CollabType);
            if (collabInfo == null)
            {
                // should not happen
                this.LogError($"CollabInfo not found for {provider.CollabType:G}");
                yield break;
            }

            var episodeName = MasterScenarioInfo.Instance.Data
                .FirstOrDefault(si => si.EpisodeId == collabInfo.StoryEpisodeId)?.EpisodeName;
            storyTitle.text = episodeName;
            storyTitleOutline.text = episodeName;

            // reset selection
            Select(0);
        }

        public void SetAdditionalTextOutlineColor(Color color)
        {
            storyTitleOutline.color = color;
        }
    }
}