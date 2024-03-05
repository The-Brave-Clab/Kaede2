using System;
using System.IO;
using UnityEngine;

namespace Kaede2
{
    [Serializable]
    public class GameSettings
    {
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

        private static string fileName => Path.Combine(Application.persistentDataPath, "settings.json");

        public static GameSettings Load()
        {
            if (!File.Exists(fileName)) return new GameSettings();

            var json = File.ReadAllText(fileName);
            return JsonUtility.FromJson<GameSettings>(json);
        }

        public static void Save()
        {
            var json = JsonUtility.ToJson(_instance, false);
            File.WriteAllText(fileName, json);
        }
    }
}