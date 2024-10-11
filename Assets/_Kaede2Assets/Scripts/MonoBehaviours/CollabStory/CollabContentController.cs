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
    public class CollabContentController : StoryCategorySelectableGroup, Kaede2InputAction.ICollabStoryContentActions
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
            InputManager.InputAction.CollabStoryContent.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.CollabStoryContent.RemoveCallbacks(this);
            InputManager.InputAction.CollabStoryContent.Disable();
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

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectedIndex % 2 != 1) return;
            var targetIndex = selectedIndex - 1;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex - 1);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectedIndex % 2 != 0) return;
            var targetIndex = selectedIndex + 1;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex + 1);
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectedIndex < 2) return;
            var targetIndex = selectedIndex - 2;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex - 2);
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectedIndex >= 2) return;
            var targetIndex = selectedIndex + 2;
            if (!items[targetIndex].gameObject.activeSelf) return;
            Select(selectedIndex + 2);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            storyController.ExitCollabContent();
        }
    }
}