using System.Collections.Generic;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede2.Scenario
{
    public class LogPanel : SelectableGroup
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

        [SerializeField]
        private AudioSource voicePlayer;

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

                // backup current ui hidden state
                uiHiddenState = scenarioModule.PlayerUIController.UIHidden;
                scenarioModule.PlayerUIController.UIHidden = true;

                // scroll to bottom
                scroll.verticalNormalizedPosition = 0;

                // select the last entry
                Select(items.Count - 1);
            }
            else
            {
                InputManager.InputAction.Scenario.Enable();
                InputManager.InputAction.ScenarioLog.Disable();

                // restore the ui hidden state
                if (uiHiddenState != null)
                {
                    scenarioModule.PlayerUIController.UIHidden = uiHiddenState.Value;
                    uiHiddenState = null;
                }

                voicePlayer.Stop();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Enable(false);

            scenarioModule.OnMesCommand += OnMesCommand;
            InputManager.onDeviceTypeChanged += OnInputDeviceChanged;
            InputManager.InputAction.ScenarioLog.GoBack.performed += ExitLogPanel;
            InputManager.InputAction.ScenarioLog.PlayVoice.performed += PlayCurrentSelectedVoice;
            InputManager.InputAction.ScenarioLog.Up.performed += SelectPrevious;
            InputManager.InputAction.ScenarioLog.Down.performed += SelectNext;

            OnInputDeviceChanged(InputManager.CurrentDeviceType);
        }

        private void OnDestroy()
        {
            scenarioModule.OnMesCommand -= OnMesCommand;
            InputManager.onDeviceTypeChanged -= OnInputDeviceChanged;
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.ScenarioLog.GoBack.performed -= ExitLogPanel;
                InputManager.InputAction.ScenarioLog.PlayVoice.performed -= PlayCurrentSelectedVoice;
                InputManager.InputAction.ScenarioLog.Up.performed -= SelectPrevious;
                InputManager.InputAction.ScenarioLog.Down.performed -= SelectNext;
            }
        }

        private void OnMesCommand(string speaker, string voiceId, string message)
        {
            var entry = Instantiate(logEntryPrefab, scroll.content).GetComponent<LogEntry>();
            entry.Panel = this;
            entry.gameObject.name = voiceId;
            entry.SpeakerText.font = LogEntryNameFont;
            entry.MessageText.font = LogEntryMessageFont;

            if (AudioManager.IsInvalidVoice(voiceId) || !scenarioModule.ScenarioResource.Voices.TryGetValue(voiceId, out var voice))
                voice = null;

            entry.SetContent(GetIconFromVoice(voiceId), voice, speaker, message);

            items.Add(entry);
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

        public void PlayVoice(AudioClip voice)
        {
            if (voice == null) return;

            voicePlayer.Stop();
            voicePlayer.clip = voice;
            voicePlayer.Play();
        }

        private void PlayCurrentSelectedVoice(InputAction.CallbackContext ctx)
        {
            if (SelectedItem == null) return;

            var entry = SelectedItem as LogEntry;
            if (entry == null) return;
    
            entry.PlayVoice();
        }

        private void SelectPrevious(InputAction.CallbackContext ctx)
        {
            Previous();
            UpdateScrollPositionBasedOnSelection();
        }

        private void SelectNext(InputAction.CallbackContext ctx)
        {
            Next();
            UpdateScrollPositionBasedOnSelection();
        }

        private void UpdateScrollPositionBasedOnSelection()
        {
            if (SelectedItem == null) return;

            var entry = SelectedItem as LogEntry;
            if (entry == null) return;

            var entryTransform = entry.transform as RectTransform;
            scroll.MoveItemIntoViewport(entryTransform);
        }

        private void OnInputDeviceChanged(InputDeviceType type)
        {
            scroll.verticalScrollbar.interactable = type is InputDeviceType.Touchscreen or InputDeviceType.KeyboardAndMouse;
            scroll.movementType = type switch
            {
                InputDeviceType.Touchscreen => ScrollRect.MovementType.Elastic,
                _ => ScrollRect.MovementType.Clamped,
            };
            scroll.inertia = type is InputDeviceType.Touchscreen;
        }
    }
}