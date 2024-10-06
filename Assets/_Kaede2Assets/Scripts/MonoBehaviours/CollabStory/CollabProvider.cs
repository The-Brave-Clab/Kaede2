using System;
using System.Collections;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Kaede2
{
    public class CollabProvider : RandomizedImageProvider
    {
        [SerializeField]
        private MasterCollabInfo.CollabType collabType;

        [SerializeField]
        private AssetReferenceSprite imageReference;

        public override Vector2 ImageSize => new(1920, 1080);

        public MasterCollabInfo.CollabType CollabType => collabType;
        public Sprite Image => imageReference.Asset as Sprite;

        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            yield return imageReference.LoadAssetAsync();

            ImageInfo[] images = new ImageInfo[count];
            for (int i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = collabType.ToString(),
                    Sprite = imageReference.Asset as Sprite
                };
            }

            onProvided(images);
        }

        private void OnDestroy()
        {
            imageReference.ReleaseAsset();
        }
    }
}