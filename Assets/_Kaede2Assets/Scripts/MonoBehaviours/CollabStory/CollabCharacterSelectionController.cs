using System;
using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabCharacterSelectionController : MonoBehaviour
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private SelectableGroup characterGroup;

        [SerializeField]
        private StoryCategorySelectable[] railgunCharacters;

        [SerializeField]
        private StoryCategorySelectable[] tojiCharacters;

        [SerializeField]
        private StoryCategorySelectable[] spyceCharacters;

        private CollabImageProvider provider;

        public IEnumerator Initialize(CollabImageProvider p)
        {
            IEnumerator WaitForCondition(Func<bool> condition)
            {
                while (!condition())
                    yield return null;
            }

            provider = p;

            StoryCategorySelectable[] characterSelectables = provider.CollabType switch
            {
                MasterCollabInfo.CollabType.RELEASE_THE_SPYCE => spyceCharacters,
                MasterCollabInfo.CollabType.TOJI_NO_MIKO => tojiCharacters,
                MasterCollabInfo.CollabType.TO_ARU_KAGAKU_NO_RAILGUN => railgunCharacters,
                _ => throw new ArgumentOutOfRangeException()
            };

            CoroutineGroup group = new();

            group.Add(provider.LoadCharacterVoiceBackground(s => background.sprite = s));

            foreach (var selectable in characterSelectables)
            {
                group.Add(WaitForCondition(() => selectable.Loaded));
            }

            yield return group.WaitForAll();

            group = new();

            foreach (var selectable in characterSelectables)
            {
                group.Add(selectable.Refresh());
            }

            yield return group.WaitForAll();

            foreach (var selectable in spyceCharacters)
                selectable.gameObject.SetActive(false);
            foreach (var selectable in tojiCharacters)
                selectable.gameObject.SetActive(false);
            foreach (var selectable in railgunCharacters)
                selectable.gameObject.SetActive(false);

            foreach (var selectable in characterSelectables)
                selectable.gameObject.SetActive(true);

            characterGroup.Select(characterSelectables[0]);
        }
    }
}