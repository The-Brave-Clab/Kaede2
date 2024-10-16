using System;
using TMPro;
using UnityEngine;

namespace Kaede2.Scenario.Framework.UI
{
    public class MessageBox : MonoBehaviour, IStateSavable<MessageBoxState>
    {
        [SerializeField]
        private GameObject messageBoxContainer;

        [SerializeField]
        private GameObject modeContainer;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private GameObject namePanel;

        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private GameObject messagePanel;

        [SerializeField]
        private Breathe nextMessageIndicator;

        [SerializeField]
        private GameObject autoModeIndicator;

        [SerializeField]
        private GameObject continuousModeIndicator;

        private RectTransform rt;

        private RichText currentText;

        public UIController UIController { get; set; }

        public string Message
        {
            set
            {
                currentText = value.Replace("\\n", "\n");
                displayTime =
                    (currentText.Length + 1) *
                    0.05f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.intervalForCharacterDisplay;
                // when in UI hidden mode, display immediately so that the user can skip the line with a single click
                if (hidden) displayTime = 0;
                timeStarted = Time.time;
                lastCharacterIndex = -1;
                currentCharacterIndex = 0;
                messageText.text = string.Empty;
                messageText.lineSpacing = 1f - 38f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.messageLineSpacing;

                nextMessageIndicator.gameObject.SetActive(false);
            }
        }

        public string NameTag
        {
            set => nameText.text = value;
        }

        private bool messageBoxEnabled;
        private bool namePanelEnabled;
        private bool hidden;

        // controlled by commands msg_box_show and msg_box_hide
        public bool Enabled
        {
            get => messageBoxEnabled;
            set
            {
                messageBoxEnabled = value;
                UpdateVisibility();
            }
        }

        // controlled by command msg_box_name_show
        public bool NamePanelEnabled
        {
            get => namePanelEnabled;
            set
            {
                namePanelEnabled = value;
                UpdateVisibility();
            }
        }

        // controlled by user input
        public bool Hidden
        {
            get => hidden;
            set
            {
                hidden = value;
                // when in UI hidden mode, display immediately so that the user can skip the line with a single click
                displayTime = 0;
                UpdateVisibility();
            }
        }

        public bool AutoMode
        {
            get => autoModeIndicator != null && autoModeIndicator.activeSelf;
            set
            {
                if (autoModeIndicator == null) return;
                autoModeIndicator.SetActive(value);
            }
        }

        public bool ContinuousMode
        {
            get => continuousModeIndicator != null && continuousModeIndicator.activeSelf;
            set
            {
                if (continuousModeIndicator == null) return;
                continuousModeIndicator.SetActive(value);
            }
        }

        public string DisplayText => currentText?.MacroText;

        public Vector2 Position
        {
            get => rt.anchoredPosition * -1;
            set => rt.anchoredPosition = value * -1;
        }

        public Action DisableAutoModeAction;
        public Action DisableContinuousModeAction;

        private float timeStarted = 1f;
        private float displayTime;
        private int lastCharacterIndex = -1;
        private int currentCharacterIndex = 0;

        public void EnterAutoMode()
        {
            nextMessageIndicator.gameObject.SetActive(false);
        }

        public void ExitAutoMode()
        {
            if (IsCompleteDisplayText)
            {
                nextMessageIndicator.gameObject.SetActive(true);
            }
        }

        public void SkipDisplay()
        {
            displayTime = 0;
        }

        private void Awake()
        {
            rt = GetComponent<RectTransform>();

            if (autoModeIndicator != null) autoModeIndicator.SetActive(false);
            if (continuousModeIndicator != null) continuousModeIndicator.SetActive(false);

            DisableAutoModeAction = null;
            DisableContinuousModeAction = null;

            messageBoxEnabled = false;
            namePanelEnabled = true;
            hidden = false;
            UpdateVisibility();
        }

        private void Update()
        {
            if (UIController.Module.AutoMode)
                nextMessageIndicator.gameObject.SetActive(false);
            else if (IsCompleteDisplayText)
                nextMessageIndicator.gameObject.SetActive(true);

            if (IsCompleteDisplayText)
                return;

            currentCharacterIndex = (int) (Mathf.Clamp01((Time.time - timeStarted) / displayTime) * currentText.Length);
            if (currentCharacterIndex != lastCharacterIndex)
            {
                messageText.text = currentText.Length == 0 ? string.Empty : currentText.String(currentCharacterIndex);
                lastCharacterIndex = currentCharacterIndex;
            }

            if (IsCompleteDisplayText)
            {
                if (!UIController.Module.AutoMode && !string.IsNullOrEmpty(messageText.text))
                    nextMessageIndicator.gameObject.SetActive(true);
            }
        }


        public bool IsCompleteDisplayText => currentCharacterIndex == (currentText?.Length ?? 0);

        private void UpdateVisibility()
        {
            messageBoxContainer.SetActive(!hidden && messageBoxEnabled);
            modeContainer.SetActive(!hidden);
            namePanel.SetActive(!hidden && namePanelEnabled);
        }

        public MessageBoxState GetState()
        {
            return new()
            {
                enabled = messageBoxEnabled,
                namePanelEnabled = namePanelEnabled,
                speaker = nameText.text,
                message = currentText.MacroText
            };
        }

        public void RestoreState(MessageBoxState state)
        {
            messageBoxEnabled = state.enabled;
            namePanelEnabled = state.namePanelEnabled;
            UpdateVisibility();
            nameText.text = state.speaker;
            Message = state.message;
            SkipDisplay();
        }

        public void DisableAutoMode()
        {
            DisableAutoModeAction?.Invoke();
        }

        public void DisableContinuousMode()
        {
            DisableContinuousModeAction?.Invoke();
        }
    }
}