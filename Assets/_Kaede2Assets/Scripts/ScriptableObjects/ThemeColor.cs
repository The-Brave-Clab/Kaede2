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
            [ColorUsage(false, false)]
            [SerializeField]
            private Color menuButtonHighlightTop;
            public Color MenuButtonHighlightTop => NonTransparent(menuButtonHighlightTop);
            [ColorUsage(false, false)]
            [SerializeField]
            private Color menuButtonHighlightBottom;
            public Color MenuButtonHighlightBottom => NonTransparent(menuButtonHighlightBottom);
            [ColorUsage(false, true)]
            [SerializeField]
            private Color menuButtonTextRim;
            public Color MenuButtonTextRim => NonTransparent(menuButtonTextRim);
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

        private static Color NonTransparent(Color input)
        {
            return new Color(input.r, input.g, input.b, 1);
        }
    }
}