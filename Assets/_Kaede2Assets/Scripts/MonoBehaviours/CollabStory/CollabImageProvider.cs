using System;
using System.Collections;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Kaede2
{
    public class CollabImageProvider : RandomizedImageProvider
    {
        [SerializeField]
        private MasterCollabInfo.CollabType collabType;

        [SerializeField]
        private AssetReferenceSprite categoryReference;

        [SerializeField]
        private AssetReferenceSprite backgroundReference;

        [SerializeField]
        private AssetReferenceSprite storyReference;

        [SerializeField]
        private AssetReferenceSprite selfIntroReference;

        [SerializeField]
        private AssetReferenceSprite characterVoiceReference;

        public override Vector2 ImageSize => new(1920, 1080);

        public MasterCollabInfo.CollabType CollabType => collabType;

        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            yield return categoryReference.LoadAssetAsync();

            ImageInfo[] images = new ImageInfo[count];
            for (int i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = collabType.ToString(),
                    Sprite = categoryReference.Asset as Sprite
                };
            }

            onProvided(images);
        }

        public IEnumerator LoadBackground(Action<Sprite> onLoaded)
        {
            yield return backgroundReference.LoadAssetAsync();
            onLoaded?.Invoke(backgroundReference.Asset as Sprite);
        }

        public IEnumerator LoadStory(Action<Sprite> onLoaded)
        {
            yield return storyReference.LoadAssetAsync();
            onLoaded?.Invoke(storyReference.Asset as Sprite);
        }

        public IEnumerator LoadSelfIntro(Action<Sprite> onLoaded)
        {
            yield return selfIntroReference.LoadAssetAsync();
            onLoaded?.Invoke(selfIntroReference.Asset as Sprite);
        }

        public IEnumerator LoadCharacterVoice(Action<Sprite> onLoaded)
        {
            yield return characterVoiceReference.LoadAssetAsync();
            onLoaded?.Invoke(characterVoiceReference.Asset as Sprite);
        }

        private void OnDestroy()
        {
            if (categoryReference.IsValid())
                categoryReference.ReleaseAsset();

            if (storyReference.IsValid())
                storyReference.ReleaseAsset();

            if (selfIntroReference.IsValid())
                selfIntroReference.ReleaseAsset();

            if (characterVoiceReference.IsValid())
                characterVoiceReference.ReleaseAsset();
        }
    }
}