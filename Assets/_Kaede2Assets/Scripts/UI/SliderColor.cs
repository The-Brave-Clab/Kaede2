using System;
using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class SliderColor : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image handleImage;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            handleImage.color = theme.SlideBar;
        }
    }
}