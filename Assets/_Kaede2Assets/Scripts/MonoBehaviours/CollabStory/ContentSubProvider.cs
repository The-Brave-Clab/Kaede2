using System;
using System.Collections;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class ContentSubProvider : RandomizedImageProvider
    {
        [SerializeField]
        private CollabStoryController controller;

        [SerializeField]
        private StoryCategorySelectable selectableItem;

        private Sprite image;
        private CollabImageProvider provider;

        public StoryCategorySelectable Selectable => selectableItem;

        public override Vector2 ImageSize => new(1920, 1080);
        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            ImageInfo[] images = new ImageInfo[count];
            for (int i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = "ContentSubProvider",
                    Sprite = image
                };
            }

            onProvided(images);
            yield break;
        }

        public void Initialize(CollabImageProvider provider, Sprite image)
        {
            this.provider = provider;
            this.image = image;
        }

        public void EnterStorySelection(bool isSelfIntro)
        {
            controller.EnterStorySelection(provider.CollabType, isSelfIntro);
        }
    }
}