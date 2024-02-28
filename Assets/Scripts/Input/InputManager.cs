using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using Kaede2.Utils;

namespace Kaede2.Input
{
    public class InputManager : Singleton<InputManager>
    {
        private InputUser user;
        private InputDeviceType currentDeviceType;

        public static InputUser User => Instance.user;
        public static InputDeviceType CurrentDeviceType => Instance.currentDeviceType;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            // could InputSystem.devices be empty or all null?
            var defaultDevice = InputSystem.devices.First(d => d != null);
            user = InputUser.PerformPairingWithDevice(defaultDevice);
            currentDeviceType = defaultDevice.GetDeviceType();

            InputUser.listenForUnpairedDeviceActivity = 1;
            InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        }

        private void Update()
        {
            // fix for onUnpairedDeviceUsed not being called with Touchscreen
            if (Touchscreen.current != null &&
                Touchscreen.current.wasUpdatedThisFrame &&
                currentDeviceType != InputDeviceType.Touchscreen)
            {
                ChangeInputDevice(Touchscreen.current);
            }
        }

        private static void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
        {
            var unpairedDevice = control.device;
            ChangeInputDevice(unpairedDevice);
            
        }

        private static void ChangeInputDevice(InputDevice device)
        {
            var type = device.GetDeviceType();

            // Debug.Log($"User used unpaired device: {JsonUtility.ToJson(unpairedDevice.description, true)}");

            List<InputDevice> devicesToPair = new();
            if (type == InputDeviceType.KeyboardAndMouse)
            {
                // only with keyboard and mouse we need to pair two devices
                // we assume that with one of them present, the other one is also present
                devicesToPair.AddRange(
                    InputSystem.devices.Where(
                        d => d != null && d.GetDeviceType() == InputDeviceType.KeyboardAndMouse));
            }
            else
            {
                devicesToPair.Add(device);
            }

            Instance.user.UnpairDevices();
            foreach (var d in devicesToPair)
            {
                InputUser.PerformPairingWithDevice(d, Instance.user);
            }

            Instance.currentDeviceType = type;
            Debug.Log($"User paired with device type {type:G}");

            // change UI elements here
        }
    }

    public static class InputDeviceExtension
    {
        public static InputDeviceType GetDeviceType(this InputDevice device)
        {
            if (device.layout != null &&
                device.layout.Equals("Touchscreen", StringComparison.InvariantCultureIgnoreCase))
                return InputDeviceType.Touchscreen;

            if (device.description.deviceClass != null)
            {
                if (device.description.deviceClass.Equals("Keyboard", StringComparison.InvariantCultureIgnoreCase) ||
                    device.description.deviceClass.Equals("Mouse", StringComparison.InvariantCultureIgnoreCase))
                    return InputDeviceType.KeyboardAndMouse;
            }

            if (device.description.manufacturer != null)
            {
                if (device.description.manufacturer.Contains("Nintendo", StringComparison.InvariantCultureIgnoreCase))
                    return InputDeviceType.NintendoController;
                if (device.description.manufacturer.Contains("Sony", StringComparison.InvariantCultureIgnoreCase))
                    return InputDeviceType.PlayStationController;
            }

            if (device.description.interfaceName != null)
            {
                if (device.description.interfaceName.Contains("XInput", StringComparison.InvariantCultureIgnoreCase))
                    return InputDeviceType.XboxController;
            }

            return InputDeviceType.GeneralGamepad;
        }
    }
}
