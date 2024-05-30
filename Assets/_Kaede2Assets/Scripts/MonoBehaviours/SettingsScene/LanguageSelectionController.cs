using Kaede2.Localization;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class LanguageSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private LocalizedAsset<TMP_FontAsset> font;

        [SerializeField]
        private SettingsSceneController sceneController;

        private void Awake()
        {
            int selectedIndex = -1;
            var allLocales = Locales.Load().All;
            for (var i = 0; i < allLocales.Count; i++)
            {
                var locale = allLocales[i];
                if (Equals(locale, GameSettings.CultureInfo)) selectedIndex = i;
                var selectionItem = selectionControl.Add("", () =>
                {
                    GameSettings.CultureInfo = locale;
                    LocalizationManager.CurrentLocale = locale;
                });
                Destroy(selectionItem.gameObject.GetComponent<LocalizeFontBehaviour>());
                Destroy(selectionItem.gameObject.GetComponent<LocalizeStringBehaviour>());
                selectionItem.Font = font.Get(locale);
                selectionItem.Text = locale.NativeName;
            }

            selectionControl.SelectImmediate(selectedIndex, false);
        }
    }
}