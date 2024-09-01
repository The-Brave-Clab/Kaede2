using System;
using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class LabeledListHighlight: MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image backgroundGradient;

        [SerializeField]
        private Image leftDecor;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            if (!Application.isPlaying) return;
            backgroundGradient.color = theme.MainMenuGradientColor;
            leftDecor.color = theme.MainMenuLeftDecorColor;
        }
    }
}