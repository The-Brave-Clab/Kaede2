using System.Collections.Generic;
using Kaede2.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede2.Scenario
{
    public class LogPanel : MonoBehaviour
    {
        [SerializeField]
        private PlayerScenarioModule scenarioModule;

        [SerializeField]
        private RectTransform container;

        [SerializeField]
        private ScrollRect scroll;

        [SerializeField]
        private GameObject logEntryPrefab;

        [SerializeField]
        private List<Sprite> characterIcons;

        // these will be set by LocalizeFontEvent
        public TMP_FontAsset LogEntryNameFont { get; set; }
        public TMP_FontAsset LogEntryMessageFont { get; set; }

        private bool? uiHiddenState;

        public void Enable(bool value)
        {
            container.gameObject.SetActive(value);
            scenarioModule.PlayerUIController.MesButton.gameObject.SetActive(!value);
            scenarioModule.PlayerUIController.MobileStyleMenu.gameObject.SetActive(!value);
            scenarioModule.Paused = value;

            if (value)
            {
                InputManager.InputAction.Scenario.Disable();
                InputManager.InputAction.ScenarioLog.Enable();

                uiHiddenState = scenarioModule.PlayerUIController.UIHidden;
                scenarioModule.PlayerUIController.UIHidden = true;
            }
            else
            {
                InputManager.InputAction.Scenario.Enable();
                InputManager.InputAction.ScenarioLog.Disable();

                if (uiHiddenState != null)
                {
                    scenarioModule.PlayerUIController.UIHidden = uiHiddenState.Value;
                    uiHiddenState = null;
                }
            }

            // scroll to bottom
            scroll.verticalNormalizedPosition = 0;
        }

        private void Awake()
        {
            Enable(false);

            scenarioModule.OnMesCommand += OnMesCommand;
            InputManager.InputAction.ScenarioLog.GoBack.performed += ExitLogPanel;
        }

        private void OnDestroy()
        {
            scenarioModule.OnMesCommand -= OnMesCommand;
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.ScenarioLog.GoBack.performed -= ExitLogPanel;
            }
        }

        private void OnMesCommand(string speaker, string voiceId, string message)
        {
            var entry = Instantiate(logEntryPrefab, scroll.content).GetComponent<LogEntry>();
            entry.gameObject.name = voiceId;
            entry.SpeakerText.font = LogEntryNameFont;
            entry.MessageText.font = LogEntryMessageFont;
            entry.SetContent(GetIconFromVoice(voiceId), speaker, message);
        }

        private Sprite GetIconFromVoice(string voiceId)
        {
            // voice id is in form of "ya29_7_0001" or "sn_08_0464"
            // extract the character id from it (in this case, "29" or "08" respectively)
            var characterIdStr = voiceId.Substring(2, 2);

            if (!int.TryParse(characterIdStr, out var characterId))
            {
                characterIdStr = voiceId.Substring(3, 2);
                if (!int.TryParse(characterIdStr, out characterId))
                    characterId = 0;
            }

            // icon name is in form of "adv_chara_icon_0000"
            string iconName = $"adv_chara_icon_{characterId:D4}";
            string fallbackIconName = "adv_chara_icon_0000";

            return characterIcons.Find(x => x.name == iconName) ?? characterIcons.Find(x => x.name == fallbackIconName);
        }

        private void ExitLogPanel(InputAction.CallbackContext ctx)
        {
            Enable(false);
        }
    }
}