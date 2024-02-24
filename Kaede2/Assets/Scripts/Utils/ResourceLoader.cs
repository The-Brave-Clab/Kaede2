using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Kaede2.Assets;
using Kaede2.Assets.AssetBundles;
using Object = UnityEngine.Object;

namespace Kaede2.Utils
{
    public class ResourceLoader
    {
        private class AssetBundleCacheEntry
        {
            public AssetBundleManifestData.Manifest manifest = null;
            public AssetBundle assetBundle = null;
            public Dictionary<string, Object> loadedAssets = new();
        }

        // asset bundle name -> asset bundle request
        private static Dictionary<string, AssetBundleCacheEntry> _assetBundleCache = new();

        public IEnumerator LoadAsync<T>(string path, Action<T> onFinishedCallback = null, Action<float> onProgressCallback = null) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path is empty");
                yield break;
            }

            if (!GetManifest(path, out var manifest))
            {
                Debug.LogError($"Failed to load AssetBundle for {path}");
                yield break;
            }

            if (!_assetBundleCache.TryGetValue(manifest.name, out var entry))
            {
                entry = new AssetBundleCacheEntry
                {
                    manifest = manifest
                };
                _assetBundleCache.Add(manifest.name, entry);
            }

            if (entry.assetBundle == null)
            {
                yield return DownloadAssetBundle(entry, onProgressCallback);
            }

            if (entry.assetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle");
                yield break;
            }

            T asset = LoadAssetFromBundle<T>(entry, path);
            onFinishedCallback?.Invoke(asset);
        }

        private IEnumerator DownloadAssetBundle(AssetBundleCacheEntry entry, Action<float> onProgressCallback = null)
        {
            if (entry.manifest == null)
            {
                Debug.LogError("Manifest is not loaded yet");
                yield break;
            }

            bool useWebRequest = PlatformHelper.FromRuntimePlatform() == KaedePlatform.WebGL;

            if (useWebRequest)
            {
                Uri uri = new Uri(new Uri($"{AssetBundleManifestData.BasePath}/"), entry.manifest.name);

                using UnityWebRequest request =
                    UnityWebRequestAssetBundle.GetAssetBundle(uri, entry.manifest.Hash, entry.manifest.crc);

                Debug.Log($"Downloading AssetBundle {entry.manifest.name} from {uri.AbsoluteUri}");
                request.SendWebRequest();

                while (!request.isDone)
                {
                    onProgressCallback?.Invoke(request.downloadProgress);
                    yield return null;
                }

                if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Failed to load AssetBundle {entry.manifest.name}: {request.error}");
                    yield break;
                }

                entry.assetBundle = DownloadHandlerAssetBundle.GetContent(request);
            }
            else
            {
                string path = Path.Combine(AssetBundleManifestData.BasePath, entry.manifest.name);
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path, entry.manifest.crc);

                Debug.Log($"Loading AssetBundle {entry.manifest.name} from file");
                while (!request.isDone)
                {
                    onProgressCallback?.Invoke(request.progress);
                    yield return null;
                }

                if (request.assetBundle == null)
                {
                    Debug.LogError($"Failed to load AssetBundle {entry.manifest.name}");
                    yield break;
                }

                entry.assetBundle = request.assetBundle;
            }

            onProgressCallback?.Invoke(1);
        }

        private T LoadAssetFromBundle<T>(AssetBundleCacheEntry entry, string assetPath) where T : Object
        {
            if (entry.assetBundle == null)
            {
                Debug.LogError("AssetBundle is not loaded yet");
                return null;
            }

            if (entry.loadedAssets.TryGetValue(assetPath, out var cachedAsset))
            {
                return cachedAsset as T;
            }

            var assetName = $"{AssetBundleManifestData.ResourceBasePath.ToLower()}/{assetPath}";
            var asset = entry.assetBundle.LoadAsset<T>(assetName);
            entry.loadedAssets.Add(assetPath, asset);
            return asset;
        }

        private bool GetManifest(string path, out AssetBundleManifestData.Manifest manifest)
        {
            foreach (var m in AssetBundleManifestData.Instance.manifests.Where(m => path.StartsWith(m.name)))
            {
                manifest = m;
                return true;
            }

            Debug.LogError($"Failed to find Asset Bundle manifest for {path}");
            manifest = default;
            return false;
        }
    }
}
