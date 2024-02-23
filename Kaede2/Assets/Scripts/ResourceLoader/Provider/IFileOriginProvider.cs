using System;
using System.Collections;
using System.IO;

namespace Kaede2.ResourceLoader.Provider
{
    public interface IFileOriginProvider
    {
        bool SupportStreaming { get; }
        byte[] GetData(string path);
        IEnumerator GetDataAsync(string path, Action<byte[]> onFinishedCallback, Action<float> onProgressCallback = null);
        Stream GetStream(string path);
        long GetSize(string path);
    }
}