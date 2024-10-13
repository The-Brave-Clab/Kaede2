using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Kaede2
{
    public class ResolutionChangeConfirmBox : SettingConfirmBox
    {
        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private float timeLimit = 10;

        public UnityEvent onYes;
        public UnityEvent onNo;

        private float startTime;

        [SerializeField]
        [TextArea]
        private string message;

        public string Message
        {
            get => message;
            set => message = value;
        }

        private void Awake()
        {
            yesButton.onClick.AddListener(() =>
            {
                onYes.Invoke();
                Destroy(gameObject);
            });

            noButton.onClick.AddListener(() =>
            {
                onNo.Invoke();
                Destroy(gameObject);
            });

            startTime = Time.time;
            Update();
        }

        private void Update()
        {
            messageText.text = string.Format(message, Mathf.Clamp((int)(timeLimit + startTime - Time.time + 1), 1, timeLimit));
            if (Time.time - startTime > timeLimit)
            {
                onNo.Invoke();
                Destroy(gameObject);
            }
        }
    }
}