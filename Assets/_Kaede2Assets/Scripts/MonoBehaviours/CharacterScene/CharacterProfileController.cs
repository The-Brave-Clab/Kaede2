using System;
using System.Collections;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CharacterProfileController : MonoBehaviour
    {
        [SerializeField]
        private CharacterSceneController sceneController;

        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private TextMeshProUGUI characterName;

        [SerializeField]
        private TextMeshProUGUI voiceActor;

        [SerializeField]
        private OptionButton selfIntroButton;

        [SerializeField]
        private TextMeshProUGUI gradeValueText;

        [SerializeField]
        private TextMeshProUGUI heightValueText;

        [SerializeField]
        private TextMeshProUGUI birthdayValueText;

        [SerializeField]
        private TextMeshProUGUI bloodTypeValueText;

        [SerializeField]
        private TextMeshProUGUI favoriteValueText;

        [SerializeField]
        private TextMeshProUGUI workValueText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private VoiceButton selfIntroVoiceButton;

        [SerializeField]
        private VoiceButton commonWordVoiceButton;

        [SerializeField]
        private VoiceButton morningVoiceButton;

        [SerializeField]
        private VoiceButton daytimeVoiceButton;

        [SerializeField]
        private VoiceButton eveningVoiceButton;

        [SerializeField]
        private VoiceButton nightVoiceButton;

        [SerializeField]
        private Image characterImage;

        private MasterCharaProfile.CharacterProfile profile;
        private MasterCharaInfo.CharacterInfo info;
        private MasterCharaVoice.CharacterVoice voice;

        private string selfIntroScenarioName; // there is no way of getting this from the profile itself, we need to find it from the scenario list

        private AsyncOperationHandle<Sprite> characterImageHandle;

        private void Awake()
        {
            // unlike others, we only do this once here because we can't clear listeners (we don't know if there are listeners other than the one we add)
            selfIntroButton.SelectableItem.onConfirmed.AddListener(PlaySelfIntroScenario);
        }

        public void Enter(MasterCharaProfile.CharacterProfile profile)
        {
            this.profile = profile;
            info = MasterCharaInfo.Instance.charaInfo.FirstOrDefault(ci => ci.Id == this.profile.Id);
            voice = MasterCharaVoice.Instance.charaVoice.FirstOrDefault(cv => cv.Id == this.profile.Id);

            selfIntroScenarioName = MasterScenarioCast.Instance.scenarioCast
                .Where(c => c.ScenarioName.StartsWith("os001_")) // indicates self intro scenario
                .Where(c => c.CastCharaIds.Length == 1) // only one character
                .Where(c => c.CastCharaIds[0] == profile.Id) // the character is the one we're looking for
                .Select(c => c.ScenarioName)
                .FirstOrDefault();

            CoroutineProxy.Start(EnterCoroutine());
        }

        private IEnumerator EnterCoroutine()
        {
            yield return SceneTransition.Fade(1);

            if (characterImageHandle.IsValid())
            {
                Addressables.Release(characterImageHandle);
            }

            characterImageHandle = ResourceLoader.LoadCharacterSprite(profile.StandingPic);

            // we don't take from profile itself because it's not localized and lack the whitespace
            // but we do need to remove the disambiguation part
            characterName.text = characterNames.Get(profile.Id).Split(" (")[0];
            voiceActor.text = $"CV: {profile.CharacterVoice}";

            gradeValueText.text = profile.Grade;
            heightValueText.text = profile.Height;
            birthdayValueText.text = profile.Birthday;
            bloodTypeValueText.text = profile.BloodType;
            favoriteValueText.text = profile.Food;
            workValueText.text = profile.Work.Replace("／", "\n"); // special fix for Aki Masuzu, fuck you entergram
            descriptionText.text = profile.Description;

            selfIntroVoiceButton.SetVoice(voice.Self_Voice);
            commonWordVoiceButton.SetVoice(voice.A_Word_Voice);
            morningVoiceButton.SetVoice(voice.Morning_Voice);
            daytimeVoiceButton.SetVoice(voice.Daytime_Voice);
            eveningVoiceButton.SetVoice(voice.Night_Voice);
            nightVoiceButton.SetVoice(voice.Sleep_Voice);

            sceneController.gameObject.SetActive(false);
            gameObject.SetActive(true);

            if (!characterImageHandle.IsDone)
                yield return characterImageHandle;

            characterImage.sprite = characterImageHandle.Result;

            yield return SceneTransition.Fade(0);
        }

        public void Exit()
        {
            CoroutineProxy.Start(ExitCoroutine());
        }

        private IEnumerator ExitCoroutine()
        {
            yield return SceneTransition.Fade(1);

            gameObject.SetActive(false);
            sceneController.gameObject.SetActive(true);

            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            if (characterImageHandle.IsValid())
            {
                Addressables.Release(characterImageHandle);
            }
        }

        private void PlaySelfIntroScenario()
        {
            // TODO
            this.Log($"Play scenario: {selfIntroScenarioName}");
        }
    }
}