using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Kaede2
{
    [Serializable]
    public class GameSettings
    {
        [SerializeField]
        private int openingMovie = 0; // -1: disabled, 0: random, 1: op1, 2: op2

        public static int OpeningMovie
        {
            get => _instance.openingMovie;
            set
            {
                _instance.openingMovie = value;
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