using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public abstract class HandleBase : IDisposable
        {
            protected float _progress;
            protected bool _isDone;
            protected AsyncOperationStatus _status;

            public float Progress => _progress;
            public bool IsDone => _isDone;
            public AsyncOperationStatus Status => _status;

            public abstract IEnumerator Send();

            public abstract void Dispose();
        }

        public abstract class BaseHandle<T> : HandleBase where T : Object
        {
            protected T _result;

            public T Result => _result;
        }

        public class LoadAddressableHandle<T> : BaseHandle<T> where T : Object
        {
            private readonly string _assetAddress;
            private readonly AsyncOperationHandle<T> _handle;

            internal LoadAddressableHandle(string assetAddress)
            {
                _progress = 0.0f;
                _isDone = false;
                _status = AsyncOperationStatus.None;
                _result = null;
                _assetAddress = assetAddress;
                _handle = Addressables.LoadAssetAsync<T>(assetAddress);
            }

            public override IEnumerator Send()
            {
                while (!_handle.IsDone)
                {
                    _progress = _handle.PercentComplete;
                    _status = _handle.Status;
                    yield return null;
                }

                _progress = 1.0f;
                _isDone = true;
                _status = _handle.Status;
                _result = _handle.Result;
            }

            public override void Dispose()
            {
                Addressables.Release(_handle);
            }
        }
    }
}