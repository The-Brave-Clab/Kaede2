using System;
using System.Globalization;
using Kaede2.Localization;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    [Serializable]
    public class GameSettings : SavableSingleton<GameSettings>
    {
        public enum OpeningMovieOptions
        {
            Disabled = -2,
            Random = -1,

            YuukiNoBaton = 0,
            Hanayui,
            Count
        }

        [SerializeField]
        private OpeningMovieOptions openingMovie = OpeningMovieOptions.Random;

        public static OpeningMovieOptions OpeningMovie
        {
            get => Instance.openingMovie;
            set
            {
                if (value == Instance.openingMovie) return;
                Instance.openingMovie = value;
                Save();
            }
        }

        [SerializeField]
        private int themeVolume = -1; // -1: random, 0: volume1, 1: volume2...

        public static int ThemeVolume
        {
            get => Instance.themeVolume;
            set
            {
                if (value == Instance.themeVolume) return;
                Instance.themeVolume = value;
                Theme.RuntimeThemeVolume = value;
                Save();
            }
        }

        [SerializeField]
        private bool fixed16By9 = true;

        public static bool Fixed16By9
        {
            get => Instance.fixed16By9;
            set
            {
                if (value == Instance.fixed16By9) return;
                Instance.fixed16By9 = value;
                Save();
            }
        }

        [SerializeField]
        private bool consoleStyle = true;

        public static bool ConsoleStyle
        {
            get => Instance.consoleStyle;
            set
            {
                if (value == Instance.consoleStyle) return;
                Instance.consoleStyle = value;
                Save();
            }
        }

        [SerializeField]
        private float audioMasterVolume = 1.0f;

        public static float AudioMasterVolume
        {
            get => Instance.audioMasterVolume;
            set
            {
                if (Mathf.Abs(value - Instance.audioMasterVolume) < 0.01f) return;
                Instance.audioMasterVolume = value;
                Save();
            }
        }

        [SerializeField]
        private float audioBGMVolume = 1.0f;

        public static float AudioBGMVolume
        {
            get => Instance.audioBGMVolume;
            set
            {
                if (Mathf.Abs(value - Instance.audioBGMVolume) < 0.01f) return;
                Instance.audioBGMVolume = value;
                Save();
            }
        }

        [SerializeField]
        private float audioSEVolume = 1.0f;

        public static float AudioSEVolume
        {
            get => Instance.audioSEVolume;
            set
            {
                if (Mathf.Abs(value - Instance.audioSEVolume) < 0.01f) return;
                Instance.audioSEVolume = value;
                Save();
            }
        }

        [SerializeField]
        private float audioVoiceVolume = 1.0f;

        public static float AudioVoiceVolume
        {
            get => Instance.audioVoiceVolume;
            set
            {
                if (Mathf.Abs(value - Instance.audioVoiceVolume) < 0.01f) return;
                Instance.audioVoiceVolume = value;
                Save();
            }
        }

        [SerializeField] private SerializableCultureInfo locale;

        public static CultureInfo CultureInfo
        {
            get
            {
                if (Instance.locale != null) return Instance.locale;
                Instance.locale = CommonUtils.GetSystemLocaleOrDefault();
                Save();
                return Instance.locale;

            }
            set
            {
                if (Instance.locale.Equals(value)) return;
                Instance.locale = value;
                Save();
            }
        }
    }
}