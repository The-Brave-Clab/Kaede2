using System;
using System.Globalization;
using Kaede2.Scenario.Framework.Utils;
using UnityEngine;

namespace Kaede2.Localization
{
    public class LocalizationManager : SingletonMonoBehaviour<LocalizationManager>
    {
        [SerializeField]
        private Locales locales;

        private event Action<CultureInfo> onLocaleChanged;

        public static event Action<CultureInfo> OnLocaleChanged
        {
            add
            {
                if (Instance != null) Instance.onLocaleChanged += value;
            }
            remove
            {
                if (Instance != null) Instance.onLocaleChanged -= value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (locales == null)
            {
#if UNITY_EDITOR
                locales = Locales.Load();
#else
                Debug.LogError("Locales is null! This should not happen in a build.");
                return;
#endif
            }

            currentLocale = GameSettings.CultureInfo;
        }

        private CultureInfo currentLocale;

        public static CultureInfo CurrentLocale
        {
            get => Instance.currentLocale;
            set
            {
                if (Equals(Instance.currentLocale, value)) return;
                Instance.currentLocale = value;
                Instance.onLocaleChanged?.Invoke(value);
            }
        }
    }
}