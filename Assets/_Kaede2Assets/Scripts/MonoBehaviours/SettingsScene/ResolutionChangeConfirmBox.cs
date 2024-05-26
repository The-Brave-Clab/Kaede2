using System;
using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Kaede2
{
    public class ResolutionChangeConfirmBox : MonoBehaviour
    {
        [SerializeField]
        private BoxWindow boxWindow;

        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private CommonButton yesButton;

        [SerializeField]
        private CommonButton noButton;

        [SerializeField]
        private float timeLimit = 10;

        public UnityEvent onYes;
        public UnityEvent onNo;

        private float startTime;

        [SerializeField]
        [TextArea]
        private string message;

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