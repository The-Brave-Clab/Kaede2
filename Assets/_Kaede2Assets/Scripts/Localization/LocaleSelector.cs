using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Kaede2.Localization
{
    [DisplayName("Kaede2 Locale Selector")]
    public class LocaleSelector : IStartupLocaleSelector
    {
        public Locale GetStartupLocale(ILocalesProvider availableLocales)
        {
            // on web build we don't select locale from settings
#if UNITY_WEBGL && UNITY_EDITOR
            return Locale.CreateLocale(SystemLanguage.Japanese);
#else
            var result = GameSettings.Locale;
            this.Log($"Selected locale: {result}");
            return result;
#endif
        }
    }
}