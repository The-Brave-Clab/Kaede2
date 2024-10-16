using System;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class TitleColor : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image background;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            background.color = theme.HelpThemeColor;
        }
    }
}