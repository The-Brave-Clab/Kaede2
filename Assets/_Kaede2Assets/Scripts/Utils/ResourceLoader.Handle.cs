using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public abstract class HandleBase : IDisposable
        {
            protected float progress;
            protected bool isDone;
            protected AsyncOperationStatus status;

            public float Progress => progress;
            public bool IsDone => isDone;
            public AsyncOperationStatus Status => status;

            public abstract IEnumerator Send();

            private bool disposed = false;

            public virtual void Dispose()
            {
                if (disposed)
                {
                    this.LogWarning("ResourceLoader Handle was already disposed!");
                    return;
                }
                disposed = true;
            }

            ~HandleBase()
            {
                if (disposed) return;
#if UNITY_EDITOR
                // in editor, when exiting play mode, the handle may not be disposed properly, which is normal
                bool exitingPlayMode = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && Application.isPlaying;
#else
                const bool exitingPlayMode = false;
#endif
                if (!exitingPlayMode)
                    this.LogWarning("ResourceLoader Handle was not disposed properly! This may cause memory leaks.");
                Dispose();
            }
        }

        public abstract class BaseHandle<T> : HandleBase where T : Object
        {
            protected T result;

            public T Result => result;
        }

        public class LoadAddressableHandle<T> : BaseHandle<T> where T : Object
        {
            private readonly string assetAddress;
            private readonly AsyncOperationHandle<T> handle;

            public string AssetAddress => assetAddress;

            internal LoadAddressableHandle(string assetAddress)
            {
                progress = 0.0f;
                isDone = false;
                status = AsyncOperationStatus.None;
                result = null;
                this.assetAddress = assetAddress;
                handle = Addressables.LoadAssetAsync<T>(assetAddress);
            }

            public override IEnumerator Send()
            {
                while (!handle.IsDone)
                {
                    progress = handle.PercentComplete;
                    status = handle.Status;
                    yield return null;
                }

                progress = 1.0f;
                isDone = true;
                status = handle.Status;
                result = handle.Result;
            }

            public override void Dispose()
            {
                Addressables.Release(handle);
                base.Dispose();
            }
        }
    }
}