using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Kaede2.Input;

namespace Kaede2.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextWithInputButton : MonoBehaviour
    {
        private TextMeshProUGUI textComponent;

        [SerializeField]
        [TextArea(3, 10)]
        [Tooltip("The text to be displayed. Use %Input/<ActionMapName>/<ActionName>% to display input button icon.")]
        private string text;

        private string lastText;
        private Color lastColor;
        private Action<InputDeviceType> onDeviceTypeChanged;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            lastText = "";
            lastColor = Color.clear;
            onDeviceTypeChanged = type => RefreshText();
            InputManager.onDeviceTypeChanged += onDeviceTypeChanged;
        }

        private void Update()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TextMeshProUGUI>();
                return;
            }

            Color color = textComponent.color;

            if (text == lastText && color == lastColor) return;
            lastText = text;
            lastColor = color;

            RefreshText();
        }

        private void OnDestroy()
        {
            InputManager.onDeviceTypeChanged -= onDeviceTypeChanged;
        }

        private void RefreshText()
        {
            Color color = textComponent.color;
            var targetText = text;
            // we use this pattern for a input button icon:
            // %Input/<ActionMapName>/<ActionName>%
            // for example: "Press %Input/Player/Jump% to Jump"
            // will be converted to: "Press <sprite...> to Jump"
            Regex regex = new Regex(@"%Input/(\w+)/(\w+)%");
            var matches = regex.Matches(targetText);
            foreach (Match match in matches)
            {
                // if we have multiple same matches, it might have been replaced already
                if (!targetText.Contains(match.Value))
                    continue;

                // get the action map name and action name
                string actionMapName = match.Groups[1].Value;
                string actionName = match.Groups[2].Value;

                // find the action map
#if UNITY_EDITOR
                InputActionMap actionMap;
                if (Application.isPlaying)
                {
                    actionMap = InputManager.ActionAsset.FindActionMap(actionMapName);
                }
                else
                {
                    var actionAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/Resources/Kaede2InputAction.inputactions");
                    actionMap = actionAsset.FindActionMap(actionMapName);
                }
#else
                var actionMap = InputManager.ActionAsset.FindActionMap(actionMapName);
#endif
                if (actionMap == null)
                {
                    // wrong action map name, replace with Error_ActionMapName
                    targetText = targetText.Replace(match.Value, "<color=red><b>Error_ActionMapName</b></color>");
                    continue;
                }

                // find the action
                var action = actionMap.FindAction(actionName);
                if (action == null)
                {
                    // wrong action name, replace with Error_ActionName
                    targetText = targetText.Replace(match.Value, "<color=red><b>Error_ActionName</b></color>");
                    continue;
                }

                var mask = new InputBinding
                {
#if UNITY_EDITOR
                    groups = "Keyboard&Mouse"
#else
                    groups = InputManager.User.controlScheme.GetValueOrDefault().bindingGroup
#endif
                };

#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    mask.groups = InputManager.User.controlScheme.GetValueOrDefault().bindingGroup;
                }
#endif

                HashSet<SpriteId> spriteIds = new();
                foreach (var binding in action.bindings)
                {
                    if (!mask.Matches(binding))
                        continue;

                    // <sprite="Xbox" name="button_1" color=#FF0000>
#if UNITY_EDITOR
                    InputDeviceType deviceType = InputDeviceType.KeyboardAndMouse;
                    if (Application.isPlaying)
                    {
                        deviceType = InputManager.CurrentDeviceType;
                    }
#else
                    InputDeviceType deviceType = InputManager.CurrentDeviceType;
#endif
                    SpriteId spriteId = new()
                    {
                        spriteSheetName = GetSpriteSheetNameFromDeviceType(deviceType, binding),
                        spriteName = GetSpriteNameFromBindingPath(binding)
                    };
                    spriteIds.Add(spriteId);
                }
                StringBuilder sb = new();
                foreach (var spriteId in spriteIds)
                {
                    sb.Append($"<sprite=\"{spriteId.spriteSheetName}\" name=\"{spriteId.spriteName}\" color=#{ColorUtility.ToHtmlStringRGB(color)}>");
                }
                targetText = targetText.Replace(match.Value, sb.ToString());
            }

            textComponent.text = targetText;
        }

        private static string GetSpriteSheetNameFromDeviceType(InputDeviceType deviceType, InputBinding binding)
        {
            return deviceType switch
            {
                InputDeviceType.Touchscreen => "", // this will not be used
                InputDeviceType.KeyboardAndMouse => binding.path.StartsWith("<Keyboard>") ? "KeyBoard-Filled" : "Mouse-Filled",
                InputDeviceType.DualSenseController => "DualSense-Filled",
                InputDeviceType.DualShock4Controller => "DualShock4-Filled",
                InputDeviceType.XboxOneController => "Xbox-Filled",
                InputDeviceType.SwitchProController => "Joy-Con-Filled", // TODO: currently we are using joycon sprites for switch pro controller
                InputDeviceType.GeneralGamepad => "Xbox-Filled",
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };
        }

        private static string GetSpriteNameFromBindingPath(InputBinding binding)
        {
            var path = binding.path;

            if (path.StartsWith("<Keyboard>"))
            {
                var controlName = path["<Keyboard>/".Length..];

                // first, remove "numpad" and lowercase the first letter
                Regex regex = new Regex(@"^numpad(\w+)$");
                if (regex.IsMatch(controlName))
                    controlName = controlName.Substring("numpad".Length, 1).ToLower() + controlName[("numpad".Length + 1)..];

                // "0"~"9": just return
                regex = new Regex(@"^(\d+)$");
                if (regex.IsMatch(controlName))
                    return controlName;

                // "a"~"z": use upper case
                regex = new Regex(@"^([a-z])$");
                if (regex.IsMatch(controlName))
                    return controlName.ToUpper();

                // "f1"~"f12": use "F1"~"F12"
                regex = new Regex(@"^f(\d+)$");
                if (regex.IsMatch(controlName))
                    return controlName.ToUpper();

                // "ctrl", "leftCtrl", "rightCtrl": use "Ctrl"
                if (controlName.EndsWith("ctrl", StringComparison.InvariantCultureIgnoreCase))
                    return "Ctrl";

                // "alt", "leftAlt", "rightAlt": use "Alt"
                if (controlName.EndsWith("alt", StringComparison.InvariantCultureIgnoreCase))
                    return "Alt";

                // "shift", "leftShift", "rightShift": use "Shift"
                if (controlName.EndsWith("shift", StringComparison.InvariantCultureIgnoreCase))
                    return "Shift";

                // "escape": use "Esc"
                if (controlName.Equals("escape", StringComparison.InvariantCultureIgnoreCase))
                    return "Esc";

                // "upArrow", "downArrow", "leftArrow", "rightArrow": use "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"
                if (controlName.EndsWith("arrow", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "upArrow" => "ArrowUp",
                        "downArrow" => "ArrowDown",
                        "leftArrow" => "ArrowLeft",
                        "rightArrow" => "ArrowRight",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "period": use "Punctuation"
                if (controlName.Equals("period", StringComparison.InvariantCultureIgnoreCase))
                    return "Punctuation";

                // "divide": use "Slash"
                if (controlName.Equals("divide", StringComparison.InvariantCultureIgnoreCase))
                    return "Slash";

                // "multiply": use "Asterisk"
                if (controlName.Equals("multiply", StringComparison.InvariantCultureIgnoreCase))
                    return "Asterisk";

                // "leftBracket", "rightBracket": use "Bracket-Left", "Bracket-Right"
                if (controlName.EndsWith("bracket", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "leftBracket" => "Bracket-Left",
                        "rightBracket" => "Bracket-Right",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "quote": use "SingleQuotation"
                if (controlName.Equals("quote", StringComparison.InvariantCultureIgnoreCase))
                    return "SingleQuotation";

                // "backquote": use "GraveAccent"
                if (controlName.Equals("backquote", StringComparison.InvariantCultureIgnoreCase))
                    return "GraveAccent";

                // otherwise: capitalize the first letter
                return controlName[..1].ToUpper() + controlName[1..];
            }

            if (path.StartsWith("<Mouse>"))
            {
                var controlName = path["<Mouse>/".Length..];

                // "leftButton", "rightButton", "middleButton": use "LeftClick", "RightClick", "MiddleClick"
                if (controlName.EndsWith("button", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "leftButton" => "LeftClick",
                        "rightButton" => "RightClick",
                        "middleButton" => "MiddleClick",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "delta": use "Move"
                if (controlName.Equals("delta", StringComparison.InvariantCultureIgnoreCase))
                    return "Move";

                // "position": use "Mouse"
                if (controlName.Equals("position", StringComparison.InvariantCultureIgnoreCase))
                    return "Mouse";

                // "scroll/y": use "WheelMove"
                if (controlName.Equals("scroll/y", StringComparison.InvariantCultureIgnoreCase))
                    return "WheelMove";

                return "UNKNOWN";
            }

            if (path.StartsWith("<Gamepad>"))
            {
                var controlName = path["<Gamepad>/".Length..];

                // "buttonSouth", "buttonEast", "buttonWest", "buttonNorth": use "Button-South", ...
                if (controlName.StartsWith("button"))
                {
                    return controlName switch
                    {
                        "buttonSouth" => "Button-South",
                        "buttonEast" => "Button-East",
                        "buttonWest" => "Button-West",
                        "buttonNorth" => "Button-North",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "dpad/down", "dpad/left", "dpad/right", "dpad/up": use "DPad-Down", ...
                if (controlName.StartsWith("dpad/"))
                {
                    return controlName switch
                    {
                        "dpad/down" => "DPad-Down",
                        "dpad/left" => "DPad-Left",
                        "dpad/right" => "DPad-Right",
                        "dpad/up" => "DPad-Up",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "dpad": use "DPad"
                if (controlName.Equals("dpad", StringComparison.InvariantCultureIgnoreCase))
                    return "DPad";

                // "leftStick", "rightStick": use "Stick-L", "Stick-R"
                if (controlName.EndsWith("stick", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "leftStick" => "Stick-L",
                        "rightStick" => "Stick-R",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "leftTrigger", "rightTrigger": use "Trigger-Left", "Trigger-Right"
                if (controlName.EndsWith("trigger", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "leftTrigger" => "Trigger-Left",
                        "rightTrigger" => "Trigger-Right",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "leftShoulder", "rightShoulder": use "Shoulder-Left", "Shoulder-Right"
                if (controlName.EndsWith("shoulder", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "leftShoulder" => "Shoulder-Left",
                        "rightShoulder" => "Shoulder-Right",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "leftStickPress", "rightStickPress": use "Stick-L-Press", "Stick-R-Press"
                if (controlName.EndsWith("stickPress", StringComparison.InvariantCultureIgnoreCase))
                {
                    return controlName switch
                    {
                        "leftStickPress" => "Stick-L-Press",
                        "rightStickPress" => "Stick-R-Press",
                        _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null)
                    };
                }

                // "start", "select": use "Start", "Select"
                if (controlName.Equals("start", StringComparison.InvariantCultureIgnoreCase))
                    return "Start";
                if (controlName.Equals("select", StringComparison.InvariantCultureIgnoreCase))
                    return "Select";

                return "UNKNOWN";
            }
            
            return "UNKNOWN";
        }

        private struct SpriteId : IEquatable<SpriteId>
        {
            public string spriteSheetName;
            public string spriteName;

            public bool Equals(SpriteId other)
            {
                return spriteSheetName == other.spriteSheetName && spriteName == other.spriteName;
            }

            public override bool Equals(object obj)
            {
                return obj is SpriteId other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(spriteSheetName, spriteName);
            }
        }
    }
}
