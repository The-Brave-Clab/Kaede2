using System;
using System.Collections;
using System.Linq;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainMenuSceneController : MonoBehaviour
    {
        [SerializeField]
        private Image backgroundImage;

        private ResourceLoader.LoadAddressableHandle<Sprite> handle;

        private void Awake()
        {
            var illustInfo = MasterAlbumInfo.Instance.albumInfo.First(i => i.OriginId == SaveData.MainMenuBackground);
            handle = ResourceLoader.LoadIllustration(illustInfo.AlbumName);
        }

        private IEnumerator Start()
        {
            yield return handle.Send();

            backgroundImage.color = Color.white;
            backgroundImage.sprite = handle.Result;

            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            handle?.Dispose();
        }

        public void StartScenario(string scenarioName)
        {
            StartCoroutine(StartScenarioCoroutine(scenarioName));
        }

        private IEnumerator StartScenarioCoroutine(string scenarioName)
        {
            yield return SceneTransition.Fade(1);
            yield return PlayerScenarioModule.Play(scenarioName,
                LocalizationSettings.SelectedLocale,
                // LocalizationSettings.AvailableLocales.GetLocale(new("ja")),
                LoadSceneMode.Single,
                null,
                () =>
                {
                    this.Log("Scenario finished.");
                });
        }

        public void GoToSettings()
        {
            StartCoroutine(LoadNextScene("SettingsScene"));
        }

        private IEnumerator LoadNextScene(string sceneName)
        {
            yield return SceneTransition.Fade(1);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}