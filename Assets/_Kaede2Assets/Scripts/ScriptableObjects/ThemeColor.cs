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

            [ColorUsage(false, false)]
            [SerializeField]
            private Color hoverHighlight;
            public Color HoverHighlight => NonTransparent(hoverHighlight);

            [ColorUsage(false, false)]
            [SerializeField]
            private Color hoverColor;
            public Color HoverColor => NonTransparent(hoverColor);

            [ColorUsage(false, false)]
            [SerializeField]
            private Color selectedColor;
            public Color SelectedColor => NonTransparent(selectedColor);

            [SerializeField]
            private CommonButtonColor commonButtonColor;
            public CommonButtonColor CommonButtonColor => commonButtonColor.NonTransparent();

            [SerializeField]
            private CommonButtonColor optionColor;
            public CommonButtonColor OptionColor => optionColor.NonTransparent();

            [SerializeField]
            [ColorUsage(false, false)]
            private Color windowOutline;
            public Color WindowOutline => NonTransparent(windowOutline);

            [ColorUsage(true, false)]
            [SerializeField]
            private Color buttonGuideColor;
            public Color ButtonGuideColor => buttonGuideColor;

            [SerializeField]
            [ColorUsage(false, false)]
            private Color arrowSurface;
            public Color ArrowSurface => NonTransparent(arrowSurface);

            [SerializeField]
            [ColorUsage(false, false)]
            private Color arrowShadow;
            public Color ArrowShadow => NonTransparent(arrowShadow);

            [SerializeField]
            [ColorUsage(false, false)]
            private Color arrowAlternateSurface;
            public Color ArrowAlternateSurface => NonTransparent(arrowAlternateSurface);

            [SerializeField]
            [ColorUsage(false, false)]
            private Color arrowAlternateShadow;
            public Color ArrowAlternateShadow => NonTransparent(arrowAlternateShadow);

            [SerializeField]
            [ColorUsage(false, false)]
            private Color arrowWithDecor;
            public Color ArrowWithDecor => NonTransparent(arrowWithDecor);

            [SerializeField]
            [ColorUsage(false, false)]
            private Color slideBar;
            public Color SlideBar => NonTransparent(slideBar);

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

            [Header("Settings")]
            [SerializeField]
            private CommonButtonColor settingsItemColor;
            public CommonButtonColor SettingsItemColor => settingsItemColor.NonTransparent();

            [SerializeField]
            [ColorUsage(false, false)]
            private Color sliderControlActiveFillColor;
            public Color SliderControlActiveFillColor => NonTransparent(sliderControlActiveFillColor);

            [Header("Album")]
            [SerializeField]
            [ColorUsage(true, false)]
            private Color favGradientTop;
            public Color FavGradientTop => favGradientTop;

            [SerializeField]
            [ColorUsage(false, false)]
            private Color selectionOutline;
            public Color SelectionOutline => NonTransparent(selectionOutline);

            [Header("Selection")]
            [SerializeField]
            private AdjustHSV.Adjustment selectionOverlay;
            public AdjustHSV.Adjustment SelectionOverlay => selectionOverlay;

            [Header("Character")]
            [SerializeField]
            private AdjustHSV.Adjustment characterProfile;
            public AdjustHSV.Adjustment CharacterProfile => characterProfile;

            [SerializeField]
            private AdjustHSV.Adjustment profileSceneBackground;
            public AdjustHSV.Adjustment ProfileSceneBackground => profileSceneBackground;

            [SerializeField]
            [ColorUsage(false, false)]
            private Color voiceButtonCircle;
            public Color VoiceButtonCircle => NonTransparent(voiceButtonCircle);
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
                bool random = value < 0 || value >= Instance.vol.Length;
                var newVal = random ? Random.Range(0, Instance.vol.Length) : value;
                if (newVal == Instance.runtimeThemeVolume) return;
                Instance.runtimeThemeVolume = newVal;
                var logStr = $"Changing theme to volume {Instance.runtimeThemeVolume + 1}";
                if (random)
                    logStr += " (random)";
                Instance.Log(logStr);

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