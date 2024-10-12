using System;
using System.Collections.Generic;
using Kaede2.Input;
using UnityEngine;

namespace Kaede2.UI
{
    public class ShowOnSpecificInputDevice : MonoBehaviour
    {
        [Flags]
        private enum DeviceFlag
        {
            KeyboardAndMouse = 1 << 0,
            Touchscreen = 1 << 1,
            Gamepad = 1 << 2,
        }

        [SerializeField]
        private DeviceFlag inputDeviceTypes;

        private void Awake()
        {
            InputManager.onDeviceTypeChanged += OnDeviceTypeChanged;
            OnDeviceTypeChanged(InputManager.CurrentDeviceType);
        }

        private void OnDestroy()
        {
            InputManager.onDeviceTypeChanged -= OnDeviceTypeChanged;
        }

        private void OnDeviceTypeChanged(InputDeviceType type)
        {
            var currentDevice = FromInputDeviceType(type);
            gameObject.SetActive(inputDeviceTypes.HasFlag(currentDevice));
        }

        private DeviceFlag FromInputDeviceType(InputDeviceType type)
        {
            switch (type)
            {
                case InputDeviceType.KeyboardAndMouse:
                    return DeviceFlag.KeyboardAndMouse;
                case InputDeviceType.Touchscreen:
                    return DeviceFlag.Touchscreen;
                case InputDeviceType.XboxOneController:
                case InputDeviceType.DualShock4Controller:
                case InputDeviceType.DualSenseController:
                case InputDeviceType.SwitchProController:
                case InputDeviceType.GeneralGamepad:
                    return DeviceFlag.Gamepad;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}