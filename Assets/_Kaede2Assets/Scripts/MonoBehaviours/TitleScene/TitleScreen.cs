using System;
using System.Collections;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CommonUtils = Kaede2.Utils.CommonUtils;

namespace Kaede2
{
    public class TitleScreen : MonoBehaviour, IThemeChangeObserver, Kaede2InputAction.ITitleScreenActions
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private SelectableGroup selectableGroup;

        private static bool titleVoicePlayed = false;

        private void Awake()
        {
            OnThemeChange(Theme.Current);

            InputManager.InputAction.TitleScreen.Enable();
            InputManager.InputAction.TitleScreen.AddCallbacks(this);

            AudioManager.PlayBGM(SaveData.BGMName);
            if (titleVoicePlayed) return;
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.Title);
            titleVoicePlayed = true;
        }

        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);
        }

        private void OnDestroy()
        {
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.TitleScreen.RemoveCallbacks(this);
                InputManager.InputAction.TitleScreen.Disable();
            }
        }

        public void MainMenuConfirm()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.EnterMainMenu);
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        public void SettingsConfirm()
        {
            var currentSceneName = gameObject.scene.name;
            SettingsSceneController.goBackAction += () =>
            {
                AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.SaveSettings);
                CommonUtils.LoadNextScene(currentSceneName, LoadSceneMode.Single);
            };
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.Settings);
            CommonUtils.LoadNextScene("SettingsScene", LoadSceneMode.Single);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            background.sprite = Theme.Current.titleBackground;
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            selectableGroup.Previous();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            selectableGroup.Next();
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            selectableGroup.SelectedItem.Confirm();
        }
    }
}