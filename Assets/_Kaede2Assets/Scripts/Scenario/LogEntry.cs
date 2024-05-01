using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario
{
    public class LogEntry : MonoBehaviour
    {
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI speakerText;

        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private Button playVoiceButton;

        public TextMeshProUGUI SpeakerText => speakerText;
        public TextMeshProUGUI MessageText => messageText;

        public LogPanel Panel { get; set; }

        public void SetContent(Sprite icon, AudioClip voice, string speaker, string message)
        {
            iconImage.sprite = icon;
            speakerText.text = speaker;

            // if the message has only one line, add a new line
            if (message.Split("\\n").Length == 1)
                message += "\\n ";

            if (voice != null)
            {
                playVoiceButton.gameObject.SetActive(true);
                playVoiceButton.onClick.AddListener(() =>
                {
                    Panel.PlayVoice(voice);
                });
            }
            else
            {
                playVoiceButton.gameObject.SetActive(false);
            }

            messageText.text = message;
        }
    }
}