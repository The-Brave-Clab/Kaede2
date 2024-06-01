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
            InputManager.InputAction.GeneralUI.Cancel.performed += GoBack;
        }

        private void OnDisable()
        {
            InputManager.InputAction.GeneralUI.Cancel.performed -= GoBack;
        }

        private void GoBack(InputAction.CallbackContext obj)
        {
            buttonGuide.Invoke();
        }
    }
}