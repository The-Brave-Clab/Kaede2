using System;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainMenuIconRim : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image image;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            image.color = theme.MainTextRim;
        }
    }
}