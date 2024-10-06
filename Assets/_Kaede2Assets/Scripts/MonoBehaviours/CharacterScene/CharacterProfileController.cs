using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    public class CharacterProfileController : MonoBehaviour
    {
        [SerializeField]
        private GameObject sceneRoot;

        [SerializeField]
        private CharacterSceneController sceneController;

        [SerializeField]
        private GameObject parentObject;

        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private TextMeshProUGUI characterName;

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
            selfIntroButton.onConfirmed.AddListener(PlaySelfIntroScenario);
        }

        public void Enter(MasterCharaProfile.CharacterProfile profile)
        {
            this.profile = profile;
            info = MasterCharaInfo.Instance.Data.FirstOrDefault(ci => ci.Id == this.profile.Id);
            voice = MasterCharaVoice.Instance.Data.FirstOrDefault(cv => cv.Id == this.profile.Id);

            selfIntroScenarioName = MasterScenarioCast.Instance.Data
                .Where(c => MasterScenarioInfo.Instance.Data
                    .Where(i => i.ChapterId == 305) // indicates self intro scenario
                    .Any(i => i.ScenarioName == c.ScenarioName)) // convert from scenario info to scenario cast
                .Where(c => c.CastCharaIds.Contains(profile.Id)) // the character is the one we're looking for
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
            string localizedName = characterNames.Get(profile.Id).Split(" (")[0];
            characterName.text = $"{localizedName}\n<size=23.44>CV: {profile.CharacterVoice}</size>";

            selfIntroButton.Interactable = !string.IsNullOrEmpty(selfIntroScenarioName);

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
            parentObject.SetActive(true);

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

            parentObject.SetActive(false);
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
            CoroutineProxy.Start(EnterScenario(selfIntroScenarioName, LocalizationManager.AllLocales.First()));
        }

        private IEnumerator EnterScenario(string scenario, CultureInfo language)
        {
            yield return SceneTransition.Fade(1);

            sceneRoot.SetActive(false);
            yield return PlayerScenarioModule.Play(
                scenario,
                language,
                LoadSceneMode.Additive,
                null,
                BackToProfile
            );
        }

        private void BackToProfile()
        {
            CoroutineProxy.Start(BackToProfileCoroutine());
        }

        private IEnumerator BackToProfileCoroutine()
        {
            yield return PlayerScenarioModule.Unload();

            sceneRoot.SetActive(true);

            yield return SceneTransition.Fade(0);
        }
    }
}