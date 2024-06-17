using System;
using System.Collections;
using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario;
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
            var illustInfo = MasterAlbumInfo.Instance.albumInfo.First(i => i.OriginId == SaveData.MainMenuBackground);
            handle = ResourceLoader.LoadIllustration(illustInfo.AlbumName);
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

        public void StartScenario(string scenarioName)
        {
            StartCoroutine(StartScenarioCoroutine(scenarioName));
        }

        private IEnumerator StartScenarioCoroutine(string scenarioName)
        {
            yield return SceneTransition.Fade(1);
            yield return PlayerScenarioModule.Play(scenarioName,
                LocalizationManager.CurrentLocale,
                // LocalizationSettings.AvailableLocales.GetLocale(new("ja")),
                LoadSceneMode.Single,
                null,
                () =>
                {
                    this.Log("Scenario finished.");
                });
        }

        public void GoToAlbum()
        {
            CommonUtils.LoadNextScene("AlbumScene", LoadSceneMode.Single);
        }

        public void GoToSettings()
        {
            var currentSceneName = gameObject.scene.name;
            SettingsSceneController.goBackAction += () =>
            {
                CommonUtils.LoadNextScene(currentSceneName, LoadSceneMode.Single);
            };
            CommonUtils.LoadNextScene("SettingsScene", LoadSceneMode.Single);
        }
    }
}