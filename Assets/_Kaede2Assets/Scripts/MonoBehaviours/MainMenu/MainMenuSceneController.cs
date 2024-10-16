using System.Collections;
using Kaede2.Audio;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainMenuSceneController : MonoBehaviour
    {
        [SerializeField]
        private Image backgroundImage;

        private AsyncOperationHandle<Sprite> handle;

        private void Awake()
        {
            handle = ResourceLoader.LoadIllustration(SaveData.MainMenuBackground.AlbumName);
        }

        private IEnumerator Start()
        {
            yield return handle;

            backgroundImage.color = Color.white;
            backgroundImage.sprite = handle.Result;

            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            Addressables.Release(handle);
        }

        // public void StartScenario(string scenarioName)
        // {
        //     StartCoroutine(StartScenarioCoroutine(scenarioName));
        // }
        //
        // private IEnumerator StartScenarioCoroutine(string scenarioName)
        // {
        //     yield return SceneTransition.Fade(1);
        //     yield return PlayerScenarioModule.Play(scenarioName,
        //         LocalizationManager.CurrentLocale,
        //         // LocalizationSettings.AvailableLocales.GetLocale(new("ja")),
        //         LoadSceneMode.Single,
        //         null,
        //         () =>
        //         {
        //             this.Log("Scenario finished.");
        //         });
        // }
        public void GoToMainStory()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.MainStory);
            CommonUtils.LoadNextScene("MainStoryScene", LoadSceneMode.Single);
        }

        public void GoToEventStory()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.EventStory);
            CommonUtils.LoadNextScene("EventStoryScene", LoadSceneMode.Single);
        }

        public void GoToCollabStory()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.EventStory);
            CommonUtils.LoadNextScene("CollabStoryScene", LoadSceneMode.Single);
        }

        public void GoToFavoriteStory()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.FavoriteStory);
            CommonUtils.LoadNextScene("FavoriteStoryScene", LoadSceneMode.Single);
        }

        public void GoToCharacter()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.CharacterProfile);
            CommonUtils.LoadNextScene("CharacterScene", LoadSceneMode.Single);
        }

        public void GoToAlbum()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.Album);
            CommonUtils.LoadNextScene("AlbumScene", LoadSceneMode.Single);
        }

        public void GoToCartoon()
        {
            AudioManager.ConfirmSound();
            CommonUtils.LoadNextScene("CartoonScene", LoadSceneMode.Single);
        }

        public void GoToSettings()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.Settings);
            var currentSceneName = gameObject.scene.name;
            SettingsSceneController.goBackAction += () =>
            {
                AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.SaveSettings);
                CommonUtils.LoadNextScene(currentSceneName, LoadSceneMode.Single);
            };
            CommonUtils.LoadNextScene("SettingsScene", LoadSceneMode.Single);
        }
    }
}