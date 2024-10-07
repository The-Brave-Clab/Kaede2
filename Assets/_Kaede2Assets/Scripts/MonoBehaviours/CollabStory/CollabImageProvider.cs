using System;
using System.Collections;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public class CollabImageProvider : RandomizedImageProvider
    {
        [SerializeField]
        private MasterCollabInfo.CollabType collabType;

        [SerializeField]
        private string categoryImageName;

        [SerializeField]
        private string backgroundImageName;

        [SerializeField]
        private string storyImageName;

        [SerializeField]
        private string selfIntroImageName;

        [SerializeField]
        private string characterVoiceImageName;

        [SerializeField]
        private string characterVoiceBackgroundImageName;

        public override Vector2 ImageSize => new(1920, 1080);

        public MasterCollabInfo.CollabType CollabType => collabType;

        private AsyncOperationHandle<Sprite> categoryImageHandle;
        private AsyncOperationHandle<Sprite> backgroundImageHandle;
        private AsyncOperationHandle<Sprite> storyImageHandle;
        private AsyncOperationHandle<Sprite> selfIntroImageHandle;
        private AsyncOperationHandle<Sprite> characterVoiceImageHandle;
        private AsyncOperationHandle<Sprite> characterVoiceBackgroundImageHandle;

        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            if (!categoryImageHandle.IsValid())
            {
                categoryImageHandle = ResourceLoader.LoadIllustration(categoryImageName);
                yield return categoryImageHandle;
            }

            ImageInfo[] images = new ImageInfo[count];
            for (int i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = collabType.ToString(),
                    Sprite = categoryImageHandle.Result
                };
            }

            onProvided(images);
        }

        public IEnumerator LoadBackground(Action<Sprite> onLoaded)
        {
            if (!backgroundImageHandle.IsValid())
            {
                backgroundImageHandle = ResourceLoader.LoadIllustration(backgroundImageName);
                yield return backgroundImageHandle;
            }
            onLoaded?.Invoke(backgroundImageHandle.Result);
        }

        public IEnumerator LoadStory(Action<Sprite> onLoaded)
        {
            if (!storyImageHandle.IsValid())
            {
                storyImageHandle = ResourceLoader.LoadIllustration(storyImageName);
                yield return storyImageHandle;
            }
            onLoaded?.Invoke(storyImageHandle.Result);
        }

        public IEnumerator LoadSelfIntro(Action<Sprite> onLoaded)
        {
            if (!selfIntroImageHandle.IsValid())
            {
                selfIntroImageHandle = ResourceLoader.LoadIllustration(selfIntroImageName);
                yield return selfIntroImageHandle;
            }
            onLoaded?.Invoke(selfIntroImageHandle.Result);
        }

        public IEnumerator LoadCharacterVoice(Action<Sprite> onLoaded)
        {
            if (!characterVoiceImageHandle.IsValid())
            {
                characterVoiceImageHandle = ResourceLoader.LoadIllustration(characterVoiceImageName);
                yield return characterVoiceImageHandle;
            }
            onLoaded?.Invoke(characterVoiceImageHandle.Result);
        }

        public IEnumerator LoadCharacterVoiceBackground(Action<Sprite> onLoaded)
        {
            if (!characterVoiceBackgroundImageHandle.IsValid())
            {
                characterVoiceBackgroundImageHandle = ResourceLoader.LoadIllustration(characterVoiceBackgroundImageName);
                yield return characterVoiceBackgroundImageHandle;
            }
            onLoaded?.Invoke(characterVoiceBackgroundImageHandle.Result);
        }

        private void OnDestroy()
        {
            if (categoryImageHandle.IsValid())
                Addressables.Release(categoryImageHandle);

            if (backgroundImageHandle.IsValid())
                Addressables.Release(backgroundImageHandle);

            if (storyImageHandle.IsValid())
                Addressables.Release(storyImageHandle);

            if (selfIntroImageHandle.IsValid())
                Addressables.Release(selfIntroImageHandle);
    
            if (characterVoiceImageHandle.IsValid())
                Addressables.Release(characterVoiceImageHandle);

            if (characterVoiceBackgroundImageHandle.IsValid())
                Addressables.Release(characterVoiceBackgroundImageHandle);
        }
    }
}