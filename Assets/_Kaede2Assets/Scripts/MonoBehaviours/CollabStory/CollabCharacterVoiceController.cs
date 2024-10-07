using System.Collections;
using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabCharacterVoiceController : SelectableGroup
    {
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
        }

        public void PlayVoice(string voiceName)
        {
            // TODO
            this.Log($"Play voice: {voiceName}");
        }

        private void OnDisable()
        {
            if (standingImageHandle.IsValid())
            {
                standingImageHandle.Release();
            }
        }
    }
}