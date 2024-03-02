using Kaede2.Scenario;
using TMPro;
using UnityEngine;

namespace Kaede2.UI.ScenarioScene
{
    public class MessageBox : MonoBehaviour, IStateSavable<MessageBoxState>
    {
        public TextMeshProUGUI nameTag;
        public TextMeshProUGUI messagePanel;
        public Breathe nextMessageIndicator;

        private RichText currentText;

        private RichText CurrentText
        {
            get => currentText ??= new RichText("");
            set => currentText = value;
        }

        public string DisplayText => currentText?.PlainText;

        private float timeStarted = 1f;
        private float displayTime;
        private int lastCharacterIndex = -1;

        public void SetText(string text)
        {
            CurrentText = new RichText(text.Replace("\\n", "\n"));
            displayTime =
                CurrentText.Length *
                0.05f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.intervalForCharacterDisplay;
            timeStarted = Time.time;
            lastCharacterIndex = -1;
            messagePanel.text = string.Empty;
            messagePanel.lineSpacing = 1f - 38f; //SingletonMonoBehaviour<ScenarioConfig>.Instance.messageLineSpacing;

            nextMessageIndicator.gameObject.SetActive(false);
        }

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
            {
                return;
            }

            int characterIndex = (int) (Mathf.Clamp01((Time.time - timeStarted) / displayTime) * CurrentText.Length);
            if (characterIndex != lastCharacterIndex)
            {
                messagePanel.text = CurrentText.Length == 0 ? string.Empty : CurrentText.Substring(0, characterIndex);
                lastCharacterIndex = characterIndex;
            }

            if (IsCompleteDisplayText)
            {
                if (!ScenarioModule.Instance.AutoMode && !string.IsNullOrEmpty(messagePanel.text))
                    nextMessageIndicator.gameObject.SetActive(true);
            }
        }


        public bool IsCompleteDisplayText => Time.time > timeStarted + displayTime;

        public MessageBoxState GetState()
        {
            return new()
            {
                enabled = gameObject.activeSelf,
                speaker = nameTag.text,
                message = CurrentText.PlainText
            };
        }

        public void RestoreState(MessageBoxState state)
        {
            gameObject.SetActive(state.enabled);
            nameTag.text = state.speaker;
            SetText(state.message);
            SkipDisplay();
        }
    }
}