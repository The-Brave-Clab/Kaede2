using System.Collections;
using System.Linq;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public class PreLoadSceneController : MonoBehaviour
    {
        [SerializeField]
        private ProgressBar progressBar;

        [SerializeField]
        private GyuukiLoading gyuukiLoading;

        private void Awake()
        {
            progressBar.gameObject.SetActive(false);
            gyuukiLoading.gameObject.SetActive(true);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f); // we wait for a small amount of time to let the user see the loading animation 
            yield return GlobalInitializer.Initialize();
            yield return DownloadAll();

            yield return SceneManager.LoadSceneAsync("SplashScreenScene", LoadSceneMode.Single);
        }

        private IEnumerator DownloadAll()
        {
            // TODO: Show a dialog to warn the user that downloading all assets will consume a lot of data

            var allKeys = GlobalInitializer.ResourceLocator.Keys.ToList();
            var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union);

            if (downloadHandle.GetDownloadStatus().TotalBytes > 0)
            {
                progressBar.gameObject.SetActive(true);
                // gyuukiLoading.gameObject.SetActive(false);
            }

            while (!downloadHandle.IsDone)
            {
                var downloadStatus = downloadHandle.GetDownloadStatus();
                progressBar.SetValue(downloadStatus.DownloadedBytes, downloadStatus.TotalBytes);
                yield return null;
            }

            if (downloadHandle.Status == AsyncOperationStatus.Failed)
            {
                // TODO: Show a dialog saying that download failed
                this.Log("Failed to download addressables");
                yield break;
            }

            this.Log("Downloaded all addressables");
            Addressables.Release(downloadHandle);

            progressBar.gameObject.SetActive(false);
            // gyuukiLoading.gameObject.SetActive(true);
        }
    }
}