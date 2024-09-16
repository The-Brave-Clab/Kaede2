using System.Globalization;
using Kaede2.Localization;
using Kaede2.Scenario.Framework;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class CharacterNames : ScriptableObject
    {
        [SerializeField]
        private SerializableDictionary<CharacterId, LocalizedString> names;

        public string Get(CharacterId id, CultureInfo cultureInfo)
        {
            return names.TryGetValue(id, out var charName) ? charName.Get(cultureInfo) : null;
        }

        public string Get(CharacterId id)
        {
            return names.TryGetValue(id, out var charName) ? charName.Get(LocalizationManager.CurrentLocale) : null;
        }
    }
}