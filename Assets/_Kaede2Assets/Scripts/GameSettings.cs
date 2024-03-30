using System;
using System.IO;
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
                _instance.openingMovie = value;
                Save();
            }
        }

        [SerializeField]
        private int themeVolume = -1; // -1: random, 0: volume1, 1: volume2...

        private int runtimeThemeVolume = -1;

        public static int ThemeVolume
        {
            get => _instance.runtimeThemeVolume;
            set
            {
                _instance.themeVolume = value;
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
                _instance.fixed16By9 = value;
                Save();
            }
        }

        [SerializeField]
        private bool consoleStyle = true;

        public static bool ConsoleStyle
        {
            get => _instance.consoleStyle;
            set
            {
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
            Debug.Log($"Selecting opening movie: {_instance.runtimeOpeningMovie}");
            _instance.runtimeThemeVolume = _instance.themeVolume == -1 ? UnityEngine.Random.Range(0, 8) : _instance.themeVolume;
            Debug.Log($"Selecting theme volume: {_instance.runtimeThemeVolume}");
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private static string FileName => Path.Combine(Application.persistentDataPath, "settings.json");
#endif

        public static GameSettings Load()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!File.Exists(FileName)) return new GameSettings();

            var json = File.ReadAllText(FileName);
            Debug.Log($"Loaded game settings from: {FileName}");
            return JsonUtility.FromJson<GameSettings>(json);
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