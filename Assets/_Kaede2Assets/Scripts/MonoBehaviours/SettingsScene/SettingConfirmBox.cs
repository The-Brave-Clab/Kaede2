using Kaede2.Input;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public abstract class SettingConfirmBox : MonoBehaviour, Kaede2InputAction.ISettingWindowActions
    {
        [SerializeField]
        protected BoxWindow boxWindow;

        [SerializeField]
        protected CommonButton yesButton;

        [SerializeField]
        protected CommonButton noButton;

        private void OnEnable()
        {
            InputManager.InputAction.Setting.Disable();
            InputManager.InputAction.SettingWindow.Enable();
            InputManager.InputAction.SettingWindow.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.SettingWindow.RemoveCallbacks(this);
            InputManager.InputAction.SettingWindow.Disable();
            InputManager.InputAction.Setting.Enable();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (yesButton.Activated)
            {
                yesButton.OnPointerExit(null);
                noButton.OnPointerEnter(null);
            }
            else
            {
                noButton.OnPointerExit(null);
                yesButton.OnPointerEnter(null);
            }
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (yesButton.Activated)
            {
                yesButton.OnPointerExit(null);
                noButton.OnPointerEnter(null);
            }
            else
            {
                noButton.OnPointerExit(null);
                yesButton.OnPointerEnter(null);
            }
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (yesButton.Activated)
            {
                yesButton.onClick.Invoke();
            }
            else if (noButton.Activated)
            {
                noButton.onClick.Invoke();
            }
            else
            {
                yesButton.OnPointerEnter(null);
                noButton.OnPointerExit(null);
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            noButton.onClick.Invoke();
        }
    }
}