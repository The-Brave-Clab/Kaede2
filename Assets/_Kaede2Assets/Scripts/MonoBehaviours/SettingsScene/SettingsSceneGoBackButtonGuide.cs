using System;
using Kaede2.Input;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public class SettingsSceneGoBackButtonGuide : MonoBehaviour
    {
        [SerializeField]
        private ButtonGuide buttonGuide;

        private void OnEnable()
        {
            InputManager.InputAction.Setting.Enable();

            InputManager.InputAction.Setting.Cancel.performed += GoBack;
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;
            InputManager.InputAction.Setting.Cancel.performed -= GoBack;

            InputManager.InputAction.Setting.Disable();
        }

        private void GoBack(InputAction.CallbackContext obj)
        {
            buttonGuide.Invoke();
        }
    }
}