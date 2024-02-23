using System;
using System.Collections;
using System.IO;
using Kaede2.Assets;
using Kaede2.Assets.AssetBundles;
using UnityEngine;

namespace Kaede2.ResourceLoader.Provider.OriginProvider
{
    // always use UnityWebRequest to read files
    public class StreamingAssetsProvider : IFileOriginProvider
    {
        public bool SupportStreaming => true;
        public byte[] GetData(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetDataAsync(string path, Action<byte[]> onFinishedCallback, Action<float> onProgressCallback = null)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(string path)
        {
            throw new NotImplementedException();
        }

        public long GetSize(string path)
        {
            throw new NotImplementedException();
        }
    }
}