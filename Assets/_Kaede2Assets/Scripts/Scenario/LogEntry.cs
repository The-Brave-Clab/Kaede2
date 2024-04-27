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

        public void SetContent(Sprite icon, string speaker, string message)
        {
            iconImage.sprite = icon;
            speakerText.text = speaker;

            // if the message has only one line, add a new line
            if (message.Split("\\n").Length == 1)
                message += "\\n ";

            messageText.text = message;
        }
    }
}