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
            get => _instance ??= new T();
            private set => _instance = value;
        }

        static SavableSingleton()
        {
            Load();
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private static string FullFilePath => Path.Combine(Application.persistentDataPath, typeof(T).Name + ".json")
            .Replace('/', Path.DirectorySeparatorChar);
#endif

        private static void Load()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!File.Exists(FullFilePath))
            {
                Instance = new T();
                Instance.Log("Created new instance");
                Save();
            }
            else
            {
                var json = File.ReadAllText(FullFilePath);
                Instance = JsonUtility.FromJson<T>(json);
                Instance.Log($"Loaded instance from {FullFilePath}");
            }
#else
            Instance = new();
#endif
        }

        protected static void Save()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            var json = JsonUtility.ToJson(Instance, false);
            File.WriteAllText(FullFilePath, json);
#endif
        }
    }
}