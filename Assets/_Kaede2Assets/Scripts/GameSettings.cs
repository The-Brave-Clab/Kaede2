using System;
using System.IO;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    [Serializable]
    public class GameSettings
    {
        [SerializeField]
        private int openingMovie = -1; // -2: disabled, -1: random, 0: op1, 1: op2

        private int runtimeOpeningMovie = -1;

        public static int OpeningMovie
        {
            get => _instance.runtimeOpeningMovie;
            set
            {
                if (value == _instance.runtimeOpeningMovie) return;
                _instance.openingMovie = value;
                Save();
            }
        }

        [SerializeField]
        private int themeVolume = -1; // -1: random, 0: volume1, 1: volume2...

        public static int ThemeVolume
        {
            get => _instance.themeVolume;
            set
            {
                if (value == _instance.themeVolume) return;
                _instance.themeVolume = value;
                Theme.RuntimeThemeVolume = value;
                Save();
            }
        }

        [SerializeField]
        private int mainMenuBackground = 950040;

        public static int MainMenuBackground
        {
            get => _instance.mainMenuBackground;
            set
            {
                if (value == _instance.mainMenuBackground) return;
                _instance.mainMenuBackground = value;
                Save();
            }
        }

        [SerializeField]
        private bool fixed16By9 = true;

        public static bool Fixed16By9
        {
            get => _instance.fixed16By9;
            set
            {
                if (value == _instance.fixed16By9) return;
                _instance.fixed16By9 = value;
                Save();
            }
        }

        [SerializeField]
        private bool consoleStyle = false;

        public static bool ConsoleStyle
        {
            get => _instance.consoleStyle;
            set
            {
                if (value == _instance.consoleStyle) return;
                _instance.consoleStyle = value;
                Save();
            }
        }

        [SerializeField]
        private float audioMasterVolume = 1.0f;

        public static float AudioMasterVolume
        {
            get => _instance.audioMasterVolume;
            set
            {
                if (Mathf.Abs(value - _instance.audioMasterVolume) < 0.01f) return;
                _instance.audioMasterVolume = value;
                Save();
            }
        }

        [SerializeField]
        private float audioBGMVolume = 1.0f;

        public static float AudioBGMVolume
        {
            get => _instance.audioBGMVolume;
            set
            {
                if (Mathf.Abs(value - _instance.audioBGMVolume) < 0.01f) return;
                _instance.audioBGMVolume = value;
                Save();
            }
        }

        [SerializeField]
        private float audioSEVolume = 1.0f;

        public static float AudioSEVolume
        {
            get => _instance.audioSEVolume;
            set
            {
                if (Mathf.Abs(value - _instance.audioSEVolume) < 0.01f) return;
                _instance.audioSEVolume = value;
                Save();
            }
        }

        [SerializeField]
        private float audioVoiceVolume = 1.0f;

        public static float AudioVoiceVolume
        {
            get => _instance.audioVoiceVolume;
            set
            {
                if (Mathf.Abs(value - _instance.audioVoiceVolume) < 0.01f) return;
                _instance.audioVoiceVolume = value;
                Save();
            }
        }

        private static GameSettings _instance;

        static GameSettings()
        {
            _instance = Load();

            // initialize runtime values
            _instance.runtimeOpeningMovie = _instance.openingMovie == -1 ? UnityEngine.Random.Range(0, 2) : _instance.openingMovie;
            _instance.Log($"Selecting opening movie: {_instance.runtimeOpeningMovie + 1}");
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private static string FileName => Path.Combine(Application.persistentDataPath, "settings.json");
#endif

        public static GameSettings Load()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!File.Exists(FileName)) return new GameSettings();

            var json = File.ReadAllText(FileName);
            var result = JsonUtility.FromJson<GameSettings>(json);
            result.Log($"Loaded game settings from: {FileName}");
            return result;
#else
            return new();
#endif
        }

        public static void Save()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            var json = JsonUtility.ToJson(_instance, false);
            File.WriteAllText(FileName, json);
#endif
        }
    }
}