using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.DualShock;
#if UNITY_IOS
using UnityEngine.InputSystem.iOS;
#endif
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL)
using UnityEngine.InputSystem.Switch;
#endif
using UnityEngine.InputSystem.XInput;

namespace Kaede2.Input
{
    public class InputManager : Singleton<InputManager>
    {
        private InputUser user;
        private InputDeviceType currentDeviceType;
        private Kaede2InputAction inputAction;

        public static InputUser User => Instance.user;
        public static InputDeviceType CurrentDeviceType => Instance.currentDeviceType;
        public static Kaede2InputAction InputAction => Instance.inputAction;

        public static event Action<InputDeviceType> onDeviceTypeChanged;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            // could InputSystem.devices be empty or all null?
            var defaultDevice = InputSystem.devices.First(d => d != null);
            user = InputUser.PerformPairingWithDevice(defaultDevice);
            currentDeviceType = defaultDevice.GetDeviceType();
            inputAction = new();
            ChangeInputDevice(defaultDevice);

            inputAction.GeneralUI.Enable();

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

            // Instance.Log($"User used unpaired device: {JsonUtility.ToJson(unpairedDevice.description, true)}");

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
            Instance.Log($"User paired with device type {type:G}");

            ChangeControlScheme(type);

            onDeviceTypeChanged?.Invoke(type);
        }

        private static void ChangeControlScheme(InputDeviceType type)
        {
            var controlScheme = type switch
            {
                InputDeviceType.KeyboardAndMouse => InputAction.KeyboardMouseScheme,
                InputDeviceType.Touchscreen => InputAction.TouchscreenScheme,
                InputDeviceType.SwitchProController => InputAction.GamepadNintendoStyleScheme,
                _ => InputAction.GamepadScheme
            };
            User.ActivateControlScheme(controlScheme);
            InputAction.bindingMask = InputBinding.MaskByGroup(controlScheme.bindingGroup);
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
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL)
                DualSenseGamepadHID => InputDeviceType.DualSenseController,
#endif
#if UNITY_IOS
                DualSenseGampadiOS => InputDeviceType.DualSenseController,
#endif
                DualShockGamepad => InputDeviceType.DualShock4Controller,
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL)
                SwitchProControllerHID => InputDeviceType.SwitchProController,
#endif
                Gamepad => InputDeviceType.GeneralGamepad,
                _ => InputDeviceType.GeneralGamepad
            };
        }
    }
}
