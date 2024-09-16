using System;
using System.IO;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    public abstract class SavableSingleton<T> where T : SavableSingleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    Load();
                return _instance;
            }
            private set => _instance = value;
        }


#if !UNITY_WEBGL || UNITY_EDITOR
        private static string FullFilePath => Path.Combine(Application.persistentDataPath, typeof(T).Name + ".json")
            .Replace('/', Path.DirectorySeparatorChar);
#endif

        public static void Load()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!File.Exists(FullFilePath))
            {
                _instance = new T();
                _instance.Log("Created new instance");
                Save();
            }
            else
            {
                var json = File.ReadAllText(FullFilePath);
                _instance = JsonUtility.FromJson<T>(json);
                _instance.Log($"Loaded instance from {FullFilePath}");
            }
#else
            _instance = new();
#endif
        }

        public static void Save()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            var json = JsonUtility.ToJson(_instance, false);
            File.WriteAllText(FullFilePath, json);
#endif
        }
    }
}