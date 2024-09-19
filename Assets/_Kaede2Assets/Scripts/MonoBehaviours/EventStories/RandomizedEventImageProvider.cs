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
    public class RandomizedEventImageProvider : RandomizedImageProvider
    {
        [SerializeField]
        private AlbumExtraInfo availableImages;

        [SerializeField]
        private AlbumExtraInfo.ImageFilter filter = AlbumExtraInfo.ImageFilter.Is16By9;

        private AsyncOperationHandle<Sprite>[] handles;

        public override Vector2 ImageSize => new(1920, 1080);

        private ImageInfo[] images;

        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            if (images != null && images.Length == count)
            {
                onProvided?.Invoke(images);
                yield break;
            }

            Clear();
            images = new ImageInfo[count];
            handles = new AsyncOperationHandle<Sprite>[count];
            
            // no duplicate images
            var illusts = availableImages.list.Where(i => i.Passes(filter)).OrderBy(_ => Random.value).ToArray();
            var info = new MasterAlbumInfo.AlbumInfo[count];
            if (illusts.Length <= 0)
            {
                Debug.LogError("No images available for event story");
                yield break;
            }

            for (var i = 0; i < count; i++)
            {
                info[i] = MasterAlbumInfo.FromAlbumName(illusts[i % illusts.Length].name);
            }

            var group = new CoroutineGroup();
            for (var i = 0; i < count; i++)
            {
                handles[i] = ResourceLoader.LoadIllustration(info[i].AlbumName, true);
                group.Add(handles[i]);
            }

            yield return group.WaitForAll();

            for (var i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = info[i].AlbumName,
                    Sprite = handles[i].Result
                };
            }

            onProvided?.Invoke(images);
        }

        private void Clear()
        {
            if (handles == null) return;
            foreach (var handle in handles)
            {
                Addressables.Release(handle);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}