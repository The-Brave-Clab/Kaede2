using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class Theme : ScriptableObject
    {
        [Serializable]
        public struct VolumeTheme
        {
            [Header("Title")]
            public Sprite titleBackground;

            [Header("Menu")]
            public Color menuButtonHighlight;
            public Color menuButtonTextRim;
        }

        [SerializeField]
        private VolumeTheme[] vol;
        public static IReadOnlyList<VolumeTheme> Vol => Instance.vol;

        private static Theme _instance;
        private static Theme Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<Theme>("Kaede2Theme");

                if (_instance != null) return _instance;

                Debug.LogError("Theme asset not found, creating new instance.");
                _instance = CreateInstance<Theme>();

                return _instance;
            }
        }
    }
}