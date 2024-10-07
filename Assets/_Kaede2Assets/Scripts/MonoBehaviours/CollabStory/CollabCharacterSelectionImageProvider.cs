using System;
using System.Collections;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public class CollabCharacterSelectionImageProvider : RandomizedImageProvider
    {
        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private string spriteName;

        [SerializeField]
        private CharacterId characterId;

        [SerializeField]
        private TextMeshProUGUI[] characterNameTexts;

        private AsyncOperationHandle<Sprite> spriteHandle;

        public CharacterId CharacterId => characterId;
        public override Vector2 ImageSize => new(1920, 1080);
        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            if (!spriteHandle.IsValid())
            {
                spriteHandle = ResourceLoader.LoadIllustration(spriteName);
                yield return spriteHandle;
            }

            ImageInfo[] images = new ImageInfo[count];
            for (int i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = spriteHandle.Result.name,
                    Sprite = spriteHandle.Result
                };
            }

            onProvided?.Invoke(images);
        }

        private void OnEnable()
        {
            var characterName = characterNames.Get(characterId);
            foreach (var text in characterNameTexts)
            {
                text.text = characterName;
            }
        }

        private void OnDestroy()
        {
            if (spriteHandle.IsValid())
            {
                Addressables.Release(spriteHandle);
            }
        }
    }
}