using System.Collections;
using Kaede2.Input;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CommonUtils = Kaede2.Utils.CommonUtils;

namespace Kaede2
{
    public class TitleScreen : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private SelectableGroup selectableGroup;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);
        }

        private void Update()
        {
            if (InputManager.InputAction.GeneralUI.NavigateDown.triggered)
                selectableGroup.Next();
            if (InputManager.InputAction.GeneralUI.NavigateUp.triggered)
                selectableGroup.Previous();
            if (InputManager.InputAction.GeneralUI.Confirm.triggered)
                selectableGroup.SelectedItem.Confirm();
        }

        public void MainMenuConfirm()
        {
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        public void SettingsConfirm()
        {
            var currentSceneName = gameObject.scene.name;
            SettingsSceneController.goBackAction += () =>
            {
                CommonUtils.LoadNextScene(currentSceneName, LoadSceneMode.Single);
            };
            CommonUtils.LoadNextScene("SettingsScene", LoadSceneMode.Single);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            background.sprite = Theme.Current.titleBackground;
        }
    }
}