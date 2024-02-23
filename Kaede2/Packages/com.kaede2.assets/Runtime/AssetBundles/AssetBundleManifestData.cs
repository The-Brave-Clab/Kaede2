using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Kaede2.Assets.AssetBundles
{
    [Serializable]
    public struct AssetBundleManifestData
    {
        [Serializable]
        public struct Manifest
        {
            public string name;
            public string hash;
            public uint crc;

            public Hash128 Hash => Hash128.Parse(hash);
        }

        public List<Manifest> manifests;

        private static bool _instanceLoaded = false;
        private static AssetBundleManifestData _instance;
        public static AssetBundleManifestData Instance
        {
            get
            {
                if (_instanceLoaded) return _instance;

                Debug.LogError("AssetBundleManifestData is not loaded yet");
                return default;
            }
        }

        public static IEnumerator LoadManifest()
        {
            Uri manifestUri = new Uri(BasePath, $"{PlatformHelper.FromRuntimePlatform()}.json");
            using UnityWebRequest request = UnityWebRequest.Get(manifestUri);
            yield return request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to load manifest: {request.error}");
                yield break;
            }

            _instance = JsonUtility.FromJson<AssetBundleManifestData>(request.downloadHandler.text);
            _instanceLoaded = true;
        }

        public static Uri BasePath
        {
            get
            {
#if UNITY_EDITOR
                // <current-project-path>/../AssetBundles/<current-editor-platform>
                DirectoryInfo projectDir = new DirectoryInfo(Application.dataPath).Parent;
                return new Uri(Path.Combine(projectDir!.Parent!.FullName, "AssetBundles", $"{PlatformHelper.FromRuntimePlatform()}"), UriKind.Absolute);
#else
                return new Uri(Application.streamingAssetsPath, UriKind.Absolute);
#endif
            }
        }
    }
}