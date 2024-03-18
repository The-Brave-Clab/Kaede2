using Kaede2.Scenario.Base;
using TMPro;
using UnityEngine;

namespace Kaede2.Scenario.UI
{
    public class MessageBox : MonoBehaviour, IStateSavable<MessageBoxState>
    {
        public TextMeshProUGUI nameTag;
        public TextMeshProUGUI messagePanel;
        public Breathe nextMessageIndicator;
        private RectTransform rt;

        private RichText currentText;

        public string Text
        {
            set
            {
                currentText = new RichText(value.Replace("\\n", "\n"));
                displayTime =
                    (currentText.Length + 1) *
                    0.05f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.intervalForCharacterDisplay;
                timeStarted = Time.time;
                lastCharacterIndex = -1;
                currentCharacterIndex = 0;
                messagePanel.text = string.Empty;
                messagePanel.lineSpacing = 1f - 38f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.messageLineSpacing;

                nextMessageIndicator.gameObject.SetActive(false);
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
        }

        private void Update()
        {
            if (ScenarioModule.Instance.AutoMode)
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
                messagePanel.text = currentText.Length == 0 ? string.Empty : currentText.String(currentCharacterIndex);
                lastCharacterIndex = currentCharacterIndex;
            }

            if (IsCompleteDisplayText)
            {
                if (!ScenarioModule.Instance.AutoMode && !string.IsNullOrEmpty(messagePanel.text))
                    nextMessageIndicator.gameObject.SetActive(true);
            }
        }


        public bool IsCompleteDisplayText => currentCharacterIndex == currentText.Length;

        public MessageBoxState GetState()
        {
            return new()
            {
                enabled = gameObject.activeSelf,
                speaker = nameTag.text,
                message = currentText.MacroText
            };
        }

        public void RestoreState(MessageBoxState state)
        {
            gameObject.SetActive(state.enabled);
            nameTag.text = state.speaker;
            Text = state.message;
            SkipDisplay();
        }
    }
}