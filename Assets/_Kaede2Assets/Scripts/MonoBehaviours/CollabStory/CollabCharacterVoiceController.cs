using System;
using System.Collections;
using System.Linq;
using Kaede2.Input;
using Kaede2.Localization;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabCharacterVoiceController : SelectableGroup, Kaede2InputAction.ICollabCharacterVoiceActions
    {
        [SerializeField]
        private CollabStoryController storyController;

        [SerializeField]
        private Image characterStandingImage;

        [SerializeField]
        private TextMeshProUGUI characterNameText;

        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private CollabVoiceButton selfIntroButton;

        [SerializeField]
        private CollabVoiceButton commonWordButton;

        [SerializeField]
        private CollabVoiceButton morningButton;

        [SerializeField]
        private CollabVoiceButton daytimeButton;

        [SerializeField]
        private CollabVoiceButton eveningButton;

        [SerializeField]
        private CollabVoiceButton nightButton;

        private MasterCollabCharaInfo.CollabCharaInfo collabCharaInfo;
        private AsyncOperationHandle<Sprite> standingImageHandle;

        public IEnumerator Initialize(CharacterId characterId)
        {
            collabCharaInfo = MasterCollabCharaInfo.Instance.Data
                .FirstOrDefault(cci => cci.Id == characterId);

            if (collabCharaInfo == null)
            {
                this.LogError($"Character ID {characterId} not found in MasterCollabCharaInfo");
                yield break;
            }

            standingImageHandle = ResourceLoader.LoadCharacterSprite(collabCharaInfo.StandingImage);

            var characterName = characterNames.Get(characterId);
            if (LocalizationManager.CurrentLocale.Name == "en")
            {
                characterName = characterName.ToUpper().Replace(" ", "\n");
            }
            characterNameText.text = characterName;
            selfIntroButton.SetVoice(collabCharaInfo.Self_Voice);
            commonWordButton.SetVoice(collabCharaInfo.A_Word_Voice);
            morningButton.SetVoice(collabCharaInfo.Morning_Voice);
            daytimeButton.SetVoice(collabCharaInfo.Daytime_Voice);
            eveningButton.SetVoice(collabCharaInfo.Night_Voice);
            nightButton.SetVoice(collabCharaInfo.Sleep_Voice);

            yield return standingImageHandle;
            characterStandingImage.sprite = standingImageHandle.Result;

            // reset selection
            Select(0);
        }

        public void PlayVoice(string voiceName)
        {
            // TODO
            this.Log($"Play voice: {voiceName}");
        }

        private void OnEnable()
        {
            InputManager.InputAction.CollabCharacterVoice.Enable();
            InputManager.InputAction.CollabCharacterVoice.AddCallbacks(this);

            lastSelection = selectedIndex;
        }

        private void OnDisable()
        {
            if (standingImageHandle.IsValid())
            {
                standingImageHandle.Release();
            }

            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.CollabCharacterVoice.RemoveCallbacks(this);
                InputManager.InputAction.CollabCharacterVoice.Disable();
            }
        }

        private int lastSelection = 0;

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Previous();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Next();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectedIndex < items.Count - 1) return;
            Select(lastSelection);
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (selectedIndex >= items.Count - 1) return;
            lastSelection = selectedIndex;
            Select(items[^1]);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            storyController.ExitCharacterVoice();
        }
    }
}