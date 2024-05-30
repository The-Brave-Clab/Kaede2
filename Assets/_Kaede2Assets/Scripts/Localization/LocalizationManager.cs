using System;
using System.Collections.Generic;
using System.Globalization;
using Kaede2.Scenario.Framework.Utils;
using UnityEngine;

namespace Kaede2.Localization
{
    public class LocalizationManager : SingletonMonoBehaviour<LocalizationManager>
    {
        [SerializeField]
        private Locales locales;

        public static IReadOnlyList<CultureInfo> AllLocales => Instance.locales.All;

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

            DontDestroyOnLoad(gameObject);

            if (locales == null)
            {
                locales = LoadAsset();
                if (locales == null)
                {
                    Debug.LogError("Locales is null and cannot be found! Please create a Locales asset in Resources folder.");
                    return;
                }
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

        private static Locales loadedLocalesAsset;
        public static Locales LoadAsset()
        {
            if (loadedLocalesAsset != null) return loadedLocalesAsset;
            loadedLocalesAsset = Resources.Load<Locales>("Locales");
            return loadedLocalesAsset;
        }
    }
}