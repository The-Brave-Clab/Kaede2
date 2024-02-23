using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kaede2.ResourceLoader.Provider;
using Object = UnityEngine.Object;

namespace Kaede2.ResourceLoader
{
    public class ResourceLoader<TFormatProvider, TOriginProvider>
        where TFormatProvider : IFileFormatProvider, new()
        where TOriginProvider : IFileOriginProvider, new()
    {
        private readonly IFileFormatProvider _formatProvider;
        private readonly IFileOriginProvider _originProvider;

        // cache group -> (resource name -> resource)
        private static Dictionary<string, Dictionary<string, Object>> ResourceCache;

        public ResourceLoader()
        {
            if (typeof(TFormatProvider) == typeof(TOriginProvider))
            {
                var unifiedProvider = new TFormatProvider();
                _formatProvider = unifiedProvider;
                _originProvider = (TOriginProvider)(unifiedProvider as object);
            }
            else
            {
                _formatProvider = new TFormatProvider();
                _originProvider = new TOriginProvider();
            }
            ResourceCache = new Dictionary<string, Dictionary<string, Object>>();
        }

        public T Load<T>(string path, string cacheGroup = null) where T : Object
        {
            // first, check cache
            if (cacheGroup != null && ResourceCache.ContainsKey(cacheGroup) && ResourceCache[cacheGroup].ContainsKey(path))
            {
                return ResourceCache[cacheGroup][path] as T;
            }

            // if not found in cache, load from origin
            bool supportStreaming = _originProvider.SupportStreaming && _formatProvider.SupportStreaming;

            T resource;
            if (supportStreaming)
            {
                using Stream stream = _originProvider.GetStream(path);
                resource = _formatProvider.Load<T>(stream, path);
            }
            else
            {
                byte[] data = _originProvider.GetData(path);
                resource = _formatProvider.Load<T>(data, path);
            }

            // cache the resource
            AddToCache(resource, path, cacheGroup);

            return resource;
        }

        public IEnumerator LoadAsync<T>(string path, Action<T> onFinishedCallback,
            Action<float> onProgressCallback = null) where T : Object
        {
            return LoadAsync(path, null, onFinishedCallback, onProgressCallback);
        }

        public IEnumerator LoadAsync<T>(string path, string cacheGroup, Action<T> onFinishedCallback,
            Action<float> onProgressCallback = null) where T : Object
        {
            // first, check cache
            if (cacheGroup != null && ResourceCache.ContainsKey(cacheGroup) && ResourceCache[cacheGroup].ContainsKey(path))
            {
                onFinishedCallback(ResourceCache[cacheGroup][path] as T);
                yield break;
            }

            // if not found in cache, load from origin
            bool supportStreaming = _originProvider.SupportStreaming && _formatProvider.SupportStreaming;

            if (supportStreaming)
            {
                using Stream stream = _originProvider.GetStream(path);
                yield return _formatProvider.LoadAsync<T>(stream, path, resource =>
                {
                    AddToCache(resource, path, cacheGroup, onFinishedCallback);
                });
            }
            else
            {
                byte[] data = Array.Empty<byte>();
                yield return _originProvider.GetDataAsync(path, d =>
                {
                    data = d;
                }, onProgressCallback);
                yield return _formatProvider.LoadAsync<T>(data, path, resource =>
                {
                    AddToCache(resource, path, cacheGroup, onFinishedCallback);
                });
            }
        }

        public void ClearCaches(string cacheGroup = null)
        {
            if (string.IsNullOrEmpty(cacheGroup))
            {
                ResourceCache.Clear();
            }
            else
            {
                ResourceCache.Remove(cacheGroup);
            }
        }

        private void AddToCache<T>(T resource, string path, string cacheGroup, Action<T> onFinishedCallback = null) where T : Object
        {
            if (string.IsNullOrEmpty(cacheGroup))
                cacheGroup = "";

            if (!ResourceCache.ContainsKey(cacheGroup))
            {
                ResourceCache[cacheGroup] = new Dictionary<string, Object>();
            }

            ResourceCache[cacheGroup][path] = resource;

            onFinishedCallback?.Invoke(resource);
        }
    }
}
