using System;
using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class SelectionOutlineColor : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image image;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            image.color = theme.SelectionOutline;
        }
    }
}