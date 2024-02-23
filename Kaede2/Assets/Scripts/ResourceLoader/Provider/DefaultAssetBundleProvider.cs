using System;
using System.Collections;
using System.IO;
using Object = UnityEngine.Object;

namespace Kaede2.ResourceLoader.Provider
{
    // Load Asset Bundles with UnityWebRequest
    public class DefaultAssetBundleProvider : IFileFormatProvider, IFileOriginProvider
    {
        public bool SupportStreaming => true;

        public byte[] GetData(string path)
        {
            // Does nothing
            return null;
        }

        public IEnumerator GetDataAsync(string path, Action<byte[]> onFinishedCallback, Action<float> onProgressCallback = null)
        {
            // Does nothing
            onFinishedCallback?.Invoke(null);
            onProgressCallback?.Invoke(1);
            yield break;
        }

        public Stream GetStream(string path)
        {
            // Does nothing
            return null;
        }

        public long GetSize(string path)
        {
            throw new NotImplementedException();
        }

        public T Load<T>(byte[] data, string path) where T : Object
        {
            throw new NotImplementedException();
        }

        public T Load<T>(Stream stream, string path) where T : Object
        {
            throw new NotImplementedException();
        }

        public IEnumerator LoadAsync<T>(byte[] data, string path, Action<T> onFinishedCallback) where T : Object
        {
            throw new NotImplementedException();
        }

        public IEnumerator LoadAsync<T>(Stream stream, string path, Action<T> onFinishedCallback) where T : Object
        {
            throw new NotImplementedException();
        }
    }
}