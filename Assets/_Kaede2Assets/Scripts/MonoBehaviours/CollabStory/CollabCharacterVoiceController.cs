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
    public class CollabCharacterVoiceController : SelectableGroup
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

            InputManager.InputAction.CollabCharacterVoice.Confirm.performed += Confirm;
            InputManager.InputAction.CollabCharacterVoice.Cancel.performed += BackToCharacterSelection;
            InputManager.InputAction.CollabCharacterVoice.Up.performed += NavigateUp;
            InputManager.InputAction.CollabCharacterVoice.Down.performed += NavigateDown;
            InputManager.InputAction.CollabCharacterVoice.Left.performed += NavigateLeft;
            InputManager.InputAction.CollabCharacterVoice.Right.performed += NavigateRight;

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
                InputManager.InputAction.CollabCharacterVoice.Confirm.performed -= Confirm;
                InputManager.InputAction.CollabCharacterVoice.Cancel.performed -= BackToCharacterSelection;
                InputManager.InputAction.CollabCharacterVoice.Up.performed -= NavigateUp;
                InputManager.InputAction.CollabCharacterVoice.Down.performed -= NavigateDown;
                InputManager.InputAction.CollabCharacterVoice.Left.performed -= NavigateLeft;
                InputManager.InputAction.CollabCharacterVoice.Right.performed -= NavigateRight;

                InputManager.InputAction.CollabCharacterVoice.Disable();
            }
        }

        private void Confirm(InputAction.CallbackContext obj)
        {
            Confirm();
        }

        private void BackToCharacterSelection(InputAction.CallbackContext obj)
        {
            storyController.ExitCharacterVoice();
        }

        private void NavigateUp(InputAction.CallbackContext obj)
        {
            Previous();
        }

        private void NavigateDown(InputAction.CallbackContext obj)
        {
            Next();
        }

        private int lastSelection = 0;
        private void NavigateLeft(InputAction.CallbackContext obj)
        {
            if (selectedIndex < items.Count - 1) return;
            Select(lastSelection);
        }

        private void NavigateRight(InputAction.CallbackContext obj)
        {
            if (selectedIndex >= items.Count - 1) return;
            lastSelection = selectedIndex;
            Select(items[^1]);
        }
    }
}