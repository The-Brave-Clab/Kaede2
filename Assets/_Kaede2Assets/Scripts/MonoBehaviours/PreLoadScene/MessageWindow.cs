using System;
using System.Collections;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public class MessageWindow : MonoBehaviour, Kaede2InputAction.IPreLoadActions
    {
        [SerializeField]
        private CommonButton yesButton;

        [SerializeField]
        private CommonButton noButton;

        private bool? result;
        public bool Result => result ?? false;

        private bool currentSelected = false;

        private void Awake()
        {
            result = null;
            currentSelected = false;

            yesButton.onClick.AddListener(Yes);
            yesButton.onActivate.AddListener(() => currentSelected = true);

            if (noButton != null)
            {
                noButton.onClick.AddListener(No);
                noButton.onActivate.AddListener(() => currentSelected = false);
            }
        }

        private void OnEnable()
        {
            result = null;
            currentSelected = false;

            AudioManager.MessageBoxSound();

            InputManager.InputAction.PreLoad.Enable();
            InputManager.InputAction.PreLoad.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.PreLoad.RemoveCallbacks(this);
            InputManager.InputAction.PreLoad.Disable();
        }

        public IEnumerator WaitForResult()
        {
            Show();
            while (result == null)
                yield return null;
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Yes()
        {
            result = true;
            AudioManager.ConfirmSound();
            Hide();
        }

        private void No()
        {
            if (noButton == null) return;

            result = false;
            AudioManager.CancelSound();
            Hide();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentSelected)
            {
                if (noButton != null)
                {
                    noButton.OnPointerEnter(null);
                    yesButton.OnPointerExit(null);
                    AudioManager.ButtonSound();
                }
            }
            else
            {
                yesButton.OnPointerEnter(null);
                if (noButton != null)
                    noButton.OnPointerExit(null);
                AudioManager.ButtonSound();
            }
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            OnLeft(context);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentSelected)
                yesButton.OnPointerClick(null);
            else if (noButton != null)
                noButton.OnPointerClick(null);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (noButton == null) return;
            noButton.OnPointerClick(null);
        }
    }
}