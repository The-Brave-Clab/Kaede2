using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using UnityEngine;

namespace Kaede2
{
    public class FilterSettings : MonoBehaviour
    {
        private static FilterSettings instance;

        [SerializeField]
        private AlbumViewController controller;

        private Dictionary<CharacterId, CharacterFilterButton> characterFilterButtons;

        private Dictionary<CharacterId, bool> characterFilter;
        private bool favoriteOnly;

        public static IReadOnlyDictionary<CharacterId, bool> CharacterFilter => instance == null ? null : instance.characterFilter;
        public static IReadOnlyDictionary<CharacterId, CharacterFilterButton> CharacterFilterButtons => instance == null ? null : instance.characterFilterButtons;

        private void Awake()
        {
            instance = this;

            characterFilterButtons = Enum.GetValues(typeof(CharacterId))
                .Cast<CharacterId>()
                .ToDictionary(id => id, _ => (CharacterFilterButton)null);
            characterFilter = Enum.GetValues(typeof(CharacterId))
                .Cast<CharacterId>()
                .ToDictionary(id => id, id => id == CharacterId.Unknown);
            favoriteOnly = false;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public static void RegisterCharacterFilterButton(CharacterId id, CharacterFilterButton button)
        {
            if (instance == null) return;

            instance.characterFilterButtons[id] = button;
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void ApplyFilter()
        {
            controller.SetFilter(Filter);
            gameObject.SetActive(false);
        }

        private bool Filter(MasterAlbumInfo.AlbumInfo info)
        {
            bool result = true;

            if (favoriteOnly)
                result = result && SaveData.FavoriteAlbumNames
                    .Any(n => n == info.AlbumName);

            result = result && characterFilter
                .Where(pair => pair.Key != CharacterId.Unknown && pair.Value)
                .Select(pair => pair.Key)
                .All(info.CastCharaIds.Contains);

            return result;
        }

        public static void SetCharacterFilter(CharacterId id, bool filter)
        {
            if (instance == null) return;

            instance.characterFilter[id] = filter;

            if (id == CharacterId.Unknown)
                instance.ResetCharacterFilter();
            else
            {
                instance.characterFilter[CharacterId.Unknown] = false;
                instance.characterFilterButtons[CharacterId.Unknown].Deactivate();
            }
        }

        private void ResetCharacterFilter()
        {
            foreach (var id in Enum.GetValues(typeof(CharacterId)).Cast<CharacterId>())
            {
                characterFilter[id] = id == CharacterId.Unknown;
                if (id != CharacterId.Unknown && characterFilterButtons[id] != null)
                    characterFilterButtons[id].Deactivate();
            }
        }

        public void SetFavoriteOnly(bool only)
        {
            favoriteOnly = only;
        }
    }
}
