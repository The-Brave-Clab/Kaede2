using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using Kaede2.Utils;

namespace Kaede2.Input
{
    public class InputManager : Singleton<InputManager>
    {
        private InputUser user;
        private InputDeviceType currentDeviceType;
        private InputActionAsset actionAsset;

        public static InputUser User => Instance.user;
        public static InputDeviceType CurrentDeviceType => Instance.currentDeviceType;
        public static InputActionAsset ActionAsset => Instance.actionAsset;

        public static event Action<InputDeviceType> onDeviceTypeChanged;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            // could InputSystem.devices be empty or all null?
            var defaultDevice = InputSystem.devices.First(d => d != null);
            user = InputUser.PerformPairingWithDevice(defaultDevice);
            currentDeviceType = defaultDevice.GetDeviceType();
            actionAsset = Resources.Load<InputActionAsset>("Kaede2InputAction");
            ChangeInputDevice(defaultDevice);

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

            ChangeControlScheme(type);

            onDeviceTypeChanged?.Invoke(type);
        }

        private static void ChangeControlScheme(InputDeviceType type)
        {
            var controlSchemeName = type switch
            {
                InputDeviceType.KeyboardAndMouse => "Keyboard&Mouse",
                InputDeviceType.Touchscreen => "Touchscreen",
                _ => "Gamepad"
            };

            var controlScheme = Instance.actionAsset.FindControlScheme(controlSchemeName);
            if (controlScheme != null) Instance.user.ActivateControlScheme(controlScheme.Value);
        }
    }

    public static class InputDeviceExtension
    {
        public static InputDeviceType GetDeviceType(this InputDevice device)
        {
            return device switch
            {
                Keyboard or Mouse => InputDeviceType.KeyboardAndMouse,
                Touchscreen => InputDeviceType.Touchscreen,
                XInputController => InputDeviceType.XboxOneController,
                DualSenseGamepadHID => InputDeviceType.DualSenseController,
                DualShockGamepad => InputDeviceType.DualShock4Controller,
                SwitchProControllerHID => InputDeviceType.SwitchProController,
                Gamepad => InputDeviceType.GeneralGamepad,
                _ => InputDeviceType.GeneralGamepad
            };
        }
    }
}
