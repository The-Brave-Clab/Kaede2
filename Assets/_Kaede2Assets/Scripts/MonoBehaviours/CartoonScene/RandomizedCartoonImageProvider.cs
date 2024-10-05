using System;
using System.Collections;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Kaede2
{
    public class RandomizedCartoonImageProvider : RandomizedImageProvider
    {
        private AsyncOperationHandle<Sprite>[] handles;
        public override Vector2 ImageSize => new(884, 632);

        private ImageInfo[] images;

        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            if (images != null && images.Length == count)
            {
                onProvided?.Invoke(images);
                yield break;
            }

            ClearHandles();
            images = new ImageInfo[count];
            handles = new AsyncOperationHandle<Sprite>[count];

            var allFrames = MasterCartoonInfo.Instance.Data
                .SelectMany(c => c.ImageNames)
                .OrderBy(_ => Random.value)
                .ToArray();

            var group = new CoroutineGroup();
            for (var i = 0; i < count; i++)
            {
                handles[i] = ResourceLoader.LoadCartoonFrame(allFrames[i % allFrames.Length]);
                group.Add(handles[i]);
            }

            yield return group.WaitForAll();

            for (var i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = allFrames[i % allFrames.Length],
                    Sprite = handles[i].Result
                };
            }

            onProvided?.Invoke(images);
        }

        private void ClearHandles()
        {
            if (handles == null) return;
            foreach (var handle in handles)
            {
                Addressables.Release(handle);
            }
        }

        private void OnDestroy()
        {
            ClearHandles();
        }
    }
}