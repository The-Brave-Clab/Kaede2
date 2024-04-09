using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Kaede2.ScriptableObjects
{
    public class Theme : ScriptableObject
    {
        [Serializable]
        public struct VolumeTheme
        {
            [Header("Common")]
            [ColorUsage(false, false)]
            [SerializeField]
            private Color mainThemeColor;
            public Color MainThemeColor => NonTransparent(mainThemeColor);
            [ColorUsage(false, true)]
            [SerializeField]
            private Color mainTextRim;
            public Color MainTextRim => NonTransparent(mainTextRim);

            [Header("Interface Common")]
            [SerializeField]
            private AdjustHSV.Adjustment interfaceTitleBackground;
            public AdjustHSV.Adjustment InterfaceTitleBackground => interfaceTitleBackground;

            [Header("Title")]
            public Sprite titleBackground;

            [Header("Menu")]
            [ColorUsage(false, false)]
            [SerializeField]
            private Color menuButtonHighlightBottom;
            public Color MenuButtonHighlightBottom => NonTransparent(menuButtonHighlightBottom);

            [Header("MainMenu")]
            [ColorUsage(false, false)]
            [SerializeField]
            private Color mainMenuGradientColor;
            public Color MainMenuGradientColor => NonTransparent(mainMenuGradientColor);
            [ColorUsage(true, false)]
            [SerializeField]
            private Color mainMenuLeftDecorColor;
            public Color MainMenuLeftDecorColor => mainMenuLeftDecorColor;
        }

        [SerializeField]
        private VolumeTheme[] vol;

        public static VolumeTheme Current => Instance.vol[RuntimeThemeVolume];

        private static Theme _instance;
        private static Theme Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = Resources.Load<Theme>("Kaede2Theme");

                if (_instance == null)
                {
                    _instance.LogError("Theme asset not found, creating new instance.");
                    _instance = CreateInstance<Theme>();
                }

                RuntimeThemeVolume = GameSettings.ThemeVolume;

                return _instance;
            }
        }

        private int runtimeThemeVolume = 0;

        public static int RuntimeThemeVolume
        {
            get => Instance.runtimeThemeVolume;
            set
            {
                var newVal = value < 0 || value >= Instance.vol.Length ? Random.Range(0, Instance.vol.Length) : value;
                if (newVal == Instance.runtimeThemeVolume) return;
                Instance.runtimeThemeVolume = newVal;
                Instance.Log($"Changing theme to volume {Instance.runtimeThemeVolume + 1}");

                var observers = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .OfType<IThemeChangeObserver>();
                foreach (var observer in observers)
                    observer.OnThemeChange(Current);
            }
        }

        private static Color NonTransparent(Color input)
        {
            return new Color(input.r, input.g, input.b, 1);
        }
    }
}