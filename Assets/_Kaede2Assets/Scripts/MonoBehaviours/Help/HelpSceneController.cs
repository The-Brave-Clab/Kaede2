using System;
using System.Collections;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Kaede2
{
    public class HelpSceneController : MonoBehaviour, Kaede2InputAction.IHelpActions
    {
        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);
        }

        private void OnEnable()
        {
            InputManager.InputAction.Help.AddCallbacks(this);
            InputManager.InputAction.Help.Enable();
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.Help.Disable();
            InputManager.InputAction.Help.RemoveCallbacks(this);
        }

        public void GoBackToTitle()
        {
            AudioManager.CancelSound();
            CommonUtils.LoadNextScene("TitleScene", LoadSceneMode.Single);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            GoBackToTitle();
        }
    }
}