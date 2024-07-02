using System.Collections.Generic;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class ArrowColor : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private RemapRGB colorComponent;

        [SerializeField]
        private List<GameObject> objects;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
            OnDeviceTypeChanged(InputManager.CurrentDeviceType);

            InputManager.onDeviceTypeChanged += OnDeviceTypeChanged;
        }

        private void OnDestroy()
        {
            InputManager.onDeviceTypeChanged -= OnDeviceTypeChanged;
        }

        private void OnDeviceTypeChanged(InputDeviceType deviceType)
        {
            // hide the arrow if the device is a touchscreen
            bool shouldHide = deviceType == InputDeviceType.Touchscreen;
            foreach (var obj in objects)
                obj.SetActive(!shouldHide);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            colorComponent.targetColorRed = theme.ArrowSurface;
            colorComponent.targetColorGreen = theme.ArrowShadow;
            colorComponent.targetColorBlue = Color.clear;
        }
    }
}