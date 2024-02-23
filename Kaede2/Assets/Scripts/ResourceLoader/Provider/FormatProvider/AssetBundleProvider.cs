using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using Kaede2.Assets;
using Kaede2.Assets.AssetBundles;
using Kaede2.ResourceLoader.Provider.OriginProvider;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Kaede2.ResourceLoader.Provider.FormatProvider
{
    public class AssetBundleProvider : IFileFormatProvider
    {
        public bool SupportStreaming => true;
        public T Load<T>(byte[] data, string path) where T : Object
        {
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(data);
            return assetBundle.LoadAsset<T>(path);
        }

        public T Load<T>(Stream stream, string path) where T : Object
        {
            AssetBundle assetBundle = AssetBundle.LoadFromStream(stream);
            return assetBundle.LoadAsset<T>(path);
        }

        public IEnumerator LoadAsync<T>(byte[] data, string path, Action<T> onFinishedCallback) where T : Object
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(data);
            if (!request.isDone)
            {
                yield return request;
            }

            AssetBundle assetBundle = request.assetBundle;
            onFinishedCallback?.Invoke(assetBundle.LoadAsset<T>(path));
        }

        public IEnumerator LoadAsync<T>(Stream stream, string path, Action<T> onFinishedCallback) where T : Object
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
            if (!request.isDone)
            {
                yield return request;
            }

            AssetBundle assetBundle = request.assetBundle;
            onFinishedCallback?.Invoke(assetBundle.LoadAsset<T>(path));
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