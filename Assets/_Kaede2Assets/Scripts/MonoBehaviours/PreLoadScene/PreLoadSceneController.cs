using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.Utils;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using CommonUtils = Kaede2.Utils.CommonUtils;

namespace Kaede2
{
    public class PreLoadSceneController : MonoBehaviour
    {
        [SerializeField]
        private ProgressBar progressBar;

        [SerializeField]
        private GyuukiLoading gyuukiLoading;

        [SerializeField]
        private MessageWindow updateRequired;

        [SerializeField]
        private DownloadConfirmWindow downloadRequired;

        [SerializeField]
        private MessageWindow networkError;

        // TODO: for now we use code to determine the internal version
        // TODO: when ready move this to a serializable object like a ScriptableObject
        private const int CURRENT_INTERNAL_VERSION = 0;

        private static string VersionFileUrl => AWS.GetUrl(AWS.PublishBucket, "version.json", AWS.DefaultRegion, true, true, false);

        private void Awake()
        {
            progressBar.gameObject.SetActive(false);
            gyuukiLoading.gameObject.SetActive(true);

            updateRequired.gameObject.SetActive(false);
            downloadRequired.gameObject.SetActive(false);
            networkError.gameObject.SetActive(false);
        }

        private IEnumerator Start()
        {
            yield return CheckUpdate();
            yield return GlobalInitializer.Initialize();

            CoroutineGroup group = new();

            group.Add(DownloadAll());
            if (!File.Exists(Supporters.LocalPath))
                group.Add(Supporters.DownloadAndSave());

            yield return group.WaitForAll();

            CoroutineProxy.Start(ScriptTranslationManager.LoadTranslations());

            CommonUtils.LoadNextScene("SplashScreenScene", LoadSceneMode.Single);
        }

        private IEnumerator DownloadAll()
        {
#if UNITY_EDITOR
            // in editor, collecting all assets with "Use Asset Database (fastest)" is too slow
            // it doesn't require downloading anyway so we skip it here
            var assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(AddressableAssetSettings)}");
            var assetPaths = assetGUIDs.Select(AssetDatabase.GUIDToAssetPath);
            var assets = assetPaths.Select(AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>).ToArray();
            var settings = assets.FirstOrDefault();
            if (settings != null)
            {
                if (settings.ActivePlayModeDataBuilder is BuildScriptFastMode)
                {
                    this.Log($"Skipping downloading addressables because the active play mode script is \"{settings.ActivePlayModeDataBuilder.Name}\"");
                    yield break;
                }
            }
#endif
            List<object> allKeys = new();
            foreach (var locator in Addressables.ResourceLocators)
            {
                allKeys.AddRange(locator.Keys);
            }

            var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union);

            if (downloadHandle.GetDownloadStatus().TotalBytes > 0)
            {
                downloadRequired.SetSize(downloadHandle.GetDownloadStatus().TotalBytes);
                yield return downloadRequired.Window.WaitForResult();

                if (!downloadRequired.Window.Result)
                {
                    // game can't proceed without downloading addressables
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    Application.Quit(0);
#endif
                }
                progressBar.gameObject.SetActive(true);
                // gyuukiLoading.gameObject.SetActive(false);

                while (!downloadHandle.IsDone)
                {
                    var downloadStatus = downloadHandle.GetDownloadStatus();
                    progressBar.SetValue(downloadStatus.DownloadedBytes, downloadStatus.TotalBytes);
                    yield return null;
                }

                if (downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    yield return networkError.WaitForResult();
                }

                this.Log("Downloaded all addressables");
                progressBar.gameObject.SetActive(false);
                // gyuukiLoading.gameObject.SetActive(true);
            }

            Addressables.Release(downloadHandle);
        }

        private IEnumerator CheckUpdate()
        {
            var request = UnityWebRequest.Get(VersionFileUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var version = JsonConvert.DeserializeObject<VersionJson>(request.downloadHandler.text);
                this.Log($"Online version: {version.version_id}, current version: {CURRENT_INTERNAL_VERSION}");
                // we have the latest version
                if (CURRENT_INTERNAL_VERSION >= version.version_id)
                    yield break;
                yield return updateRequired.WaitForResult();
                if (updateRequired.Result)
                {
                    string platform = Application.platform switch
                    {
#if UNITY_EDITOR
                        _ => "Windows"
#else
                        // TODO: add more
                        RuntimePlatform.WindowsPlayer => "Windows",
                        RuntimePlatform.Android => "Android",
                        RuntimePlatform.OSXPlayer => "macOS",
                        _ => throw new NotImplementedException($"Unsupported platform: {Application.platform}")
#endif
                    };
                    Application.OpenURL(AWS.GetUrl(AWS.PublishBucket, version.file_names[platform], AWS.DefaultRegion, true, true, true));
                    // we sneakily clear the cache here since the user have to update and the cache will be invalid anyway
                    CommonUtils.ForceDeleteCache();
                }
                // we don't allow user to continue without the latest version
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                Application.Quit(0);
#endif
            }
            else
            {
                yield return networkError.WaitForResult();
            }
        }

        /*{
               "version_id": 0,
               "file_names": {
                   "Windows": "Kaede2-win-x64.zip",
                   "Android": "Kaede2.apk"
               }
           }
           */
        private struct VersionJson
        {
            public int version_id { get; set; }
            public IDictionary<string, string> file_names { get; set; }
        }
    }
}