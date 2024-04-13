using System;
using System.Collections;
using System.Linq;
using Kaede2.Scenario;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
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

        public void GoToNextScene(string sceneName)
        {
            StartCoroutine(GoToNextSceneCoroutine(sceneName));
        }

        private IEnumerator GoToNextSceneCoroutine(string sceneName)
        {
            yield return SceneTransition.Fade(1);
            PlayerScenarioModule.GlobalScenarioName = "ms006_s011_a";
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}