using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Kaede2
{
    public class LanguageSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private LocalizedTmpFont font;

        private void Awake()
        {
            int selectedIndex = -1;
            for (var i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                if (locale == GameSettings.Locale) selectedIndex = i;
                var selectionItem = selectionControl.Add(locale.Identifier.CultureInfo.NativeName, () => GameSettings.Locale = locale);
                font.LocaleOverride = locale;
                selectionItem.Font = font.LoadAsset();
            }

            selectionControl.SelectImmediate(selectedIndex, false);
        }
    }
}