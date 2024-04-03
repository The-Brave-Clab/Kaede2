using System.Collections;
using System.Linq;
using Kaede2.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public static class DownloadAllAssets
    {
        public static IEnumerator DownloadAll(ProgressBar progressBar)
        {
            // TODO: Show a dialog to warn the user that downloading all assets will consume a lot of data

            var allKeys = GlobalInitializer.ResourceLocator.Keys.ToList();
            var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union);

            if (downloadHandle.GetDownloadStatus().TotalBytes > 0)
            {
                progressBar.gameObject.SetActive(true);
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
                typeof(DownloadAllAssets).Log("Failed to download addressables");
                yield break;
            }

            typeof(DownloadAllAssets).Log("Downloaded all addressables");
            Addressables.Release(downloadHandle);

            progressBar.gameObject.SetActive(false);
        }
    }
}