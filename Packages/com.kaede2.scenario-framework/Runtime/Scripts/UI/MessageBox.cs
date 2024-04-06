using TMPro;
using UnityEngine;

namespace Kaede2.Scenario.Framework.UI
{
    public class MessageBox : MonoBehaviour, IStateSavable<MessageBoxState>
    {
        [SerializeField]
        private GameObject messageBoxContainer;

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

        public bool Enabled
        {
            get => messageBoxContainer.activeSelf;
            set => messageBoxContainer.SetActive(value);
        }

        public bool NamePanelEnabled
        {
            get => namePanel.activeSelf;
            set => namePanel.SetActive(value);
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
        }

        private void Update()
        {
            if (UIController.Module.AutoMode)
            {
                nextMessageIndicator.gameObject.SetActive(false);
            }
            else
            {
                if (IsCompleteDisplayText)
                {
                    nextMessageIndicator.gameObject.SetActive(true);
                }
            }

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

        public MessageBoxState GetState()
        {
            return new()
            {
                enabled = gameObject.activeSelf,
                speaker = nameText.text,
                message = currentText.MacroText
            };
        }

        public void RestoreState(MessageBoxState state)
        {
            gameObject.SetActive(state.enabled);
            nameText.text = state.speaker;
            Message = state.message;
            SkipDisplay();
        }
    }
}