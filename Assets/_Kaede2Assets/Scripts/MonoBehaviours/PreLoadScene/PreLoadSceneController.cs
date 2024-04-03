using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public class PreLoadSceneController : MonoBehaviour
    {
        [SerializeField]
        private ProgressBar progressBar;

        private void Awake()
        {
            progressBar.gameObject.SetActive(false);
        }

        private IEnumerator Start()
        {
            yield return GlobalInitializer.Initialize();
            yield return DownloadAllAssets.DownloadAll(progressBar);

            yield return SceneManager.LoadSceneAsync("SplashScreenScene", LoadSceneMode.Single);
        }
    }
}