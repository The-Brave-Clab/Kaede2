using System;
using System.Collections;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Kaede2
{
    public class CollabCharacterSelectionImageProvider : RandomizedImageProvider
    {
        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private AssetReferenceSprite spriteReference;

        [SerializeField]
        private CharacterId characterId;

        [SerializeField]
        private TextMeshProUGUI[] characterNameTexts;

        public CharacterId CharacterId => characterId;
        public override Vector2 ImageSize => new(1920, 1080);
        public override IEnumerator Provide(int count, Action<ImageInfo[]> onProvided)
        {
            if (!spriteReference.IsValid())
                yield return spriteReference.LoadAssetAsync();

            ImageInfo[] images = new ImageInfo[count];
            for (int i = 0; i < count; i++)
            {
                images[i] = new ImageInfo
                {
                    Name = spriteReference.Asset.name,
                    Sprite = spriteReference.Asset as Sprite
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
            if (spriteReference.IsValid())
            {
                spriteReference.ReleaseAsset();
            }
        }
    }
}