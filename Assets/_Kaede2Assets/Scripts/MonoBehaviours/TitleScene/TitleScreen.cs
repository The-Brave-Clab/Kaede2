using System.Collections;
using Kaede2.Input;
using Kaede2.Scenario;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            StartCoroutine(LoadNextScene());
        }

        private IEnumerator LoadNextScene()
        {
            yield return SceneTransition.Fade(1);
            // PlayerScenarioModule.GlobalScenarioName = "ms006_s011_a";
            // yield return SceneManager.LoadSceneAsync("ScenarioScene", LoadSceneMode.Single);

            yield return SceneManager.LoadSceneAsync("MainMenuScene", LoadSceneMode.Single);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            background.sprite = Theme.Current.titleBackground;
        }
    }
}