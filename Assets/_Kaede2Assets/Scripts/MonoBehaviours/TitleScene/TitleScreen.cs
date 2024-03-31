using Kaede2.Input;
using Kaede2.Scenario;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private SelectableGroup selectableGroup;

        private void Awake()
        {
            background.sprite = Theme.Vol[GameSettings.ThemeVolume].titleBackground;
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
            PlayerScenarioModule.GlobalScenarioName = "ms006_s011_a";
            SceneManager.LoadScene("ScenarioScene", LoadSceneMode.Single);
        }
    }
}