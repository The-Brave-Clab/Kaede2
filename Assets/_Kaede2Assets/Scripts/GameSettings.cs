using System;
using System.IO;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    [Serializable]
    public class GameSettings : SavableSingleton<GameSettings>
    {
        [SerializeField]
        private int openingMovie = -1; // -2: disabled, -1: random, 0: op1, 1: op2

        private int runtimeOpeningMovie = -1;

        public static int OpeningMovie
        {
            get => Instance.runtimeOpeningMovie;
            set
            {
                if (value == Instance.runtimeOpeningMovie) return;
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
        private bool consoleStyle = false;

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

        static GameSettings()
        {
            // initialize runtime values
            Instance.runtimeOpeningMovie = Instance.openingMovie == -1 ? UnityEngine.Random.Range(0, 2) : Instance.openingMovie;
            Instance.Log($"Selecting opening movie: {Instance.runtimeOpeningMovie + 1}");
        }
    }
}