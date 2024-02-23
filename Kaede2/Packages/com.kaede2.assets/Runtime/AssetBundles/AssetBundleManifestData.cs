using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Kaede2.Assets.AssetBundles
{
    [Serializable]
    public class AssetBundleManifestData
    {
        [Serializable]
        public class Manifest
        {
            public string name;
            public string hash;
            public uint crc;

            public Hash128 Hash => Hash128.Parse(hash);
        }

        public List<Manifest> manifests;

        private static AssetBundleManifestData _instance = null;
        public static AssetBundleManifestData Instance
        {
            get
            {
                if (_instance != null) return _instance;

                Debug.LogError("AssetBundleManifestData is not loaded yet");
                return null;
            }
        }

        public static IEnumerator LoadManifest()
        {
            Uri baseUri = new Uri($"{BasePath}/");
            Uri manifestUri = new Uri(baseUri, $"{ManifestFileName}");
            using UnityWebRequest request = UnityWebRequest.Get(manifestUri);
            yield return request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to load Asset Bundle manifest: {request.error}");
                yield break;
            }

            _instance = JsonUtility.FromJson<AssetBundleManifestData>(request.downloadHandler.text);

            Debug.Log($"Loaded Asset Bundle manifest with {_instance.manifests.Count} entries");
        }

        public static string BasePath
        {
            get
            {
#if UNITY_EDITOR
                // <current-project-path>/../AssetBundles/<current-editor-platform>
                DirectoryInfo projectDir = new DirectoryInfo(Application.dataPath).Parent;
                return Path.Combine(projectDir!.Parent!.FullName, "AssetBundles", $"{PlatformHelper.FromRuntimePlatform():G}");
#elif UNITY_WEBGL
                return new Uri(Application.streamingAssetsPath).AbsoluteUri;
#else
                return Application.streamingAssetsPath;
#endif
            }
        }

        public static string ResourceBasePath => "Assets/AssetBundles";

        public static string ManifestFileName => $"{PlatformHelper.FromRuntimePlatform():G}.json";
    }
}