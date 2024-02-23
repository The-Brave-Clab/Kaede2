using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaede2.ResourceLoader.Provider
{
    public interface IFileFormatProvider
    {
        bool SupportStreaming { get; }
        T Load<T>(byte[] data, string path) where T : Object;
        T Load<T>(Stream stream, string path) where T : Object;
        IEnumerator LoadAsync<T>(byte[] data, string path, Action<T> onFinishedCallback) where T : Object;
        IEnumerator LoadAsync<T>(Stream stream, string path, Action<T> onFinishedCallback) where T : Object;
    }
}