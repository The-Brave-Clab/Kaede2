using System;
using Kaede2.Input;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2.Scenario
{
    public class LogEntry : SelectableItem
    {
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI speakerText;

        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private Button touchStylePlayVoiceButton;

        [SerializeField]
        private Image gamepadStyleUpArrow;

        [SerializeField]
        private Image gamepadStyleDownArrow;

        [SerializeField]
        private TextMeshProUGUI gamepadStyleVoicePlayButtonText;

        [SerializeField]
        private Image gamepadStyleVoiceIcon;

        public TextMeshProUGUI SpeakerText => speakerText;
        public TextMeshProUGUI MessageText => messageText;

        public LogPanel Panel { get; set; }

        private AudioClip voiceClip = null;

        public void SetContent(Sprite icon, AudioClip voice, string speaker, string message)
        {
            // if the message has only one line, add a new line
            if (message.Split("\\n").Length == 1)
                message += "\\n ";

            iconImage.sprite = icon;
            speakerText.text = speaker;
            voiceClip = voice;

            messageText.text = message;

            UpdatePlayVoiceIconsVisibility();

            touchStylePlayVoiceButton.onClick.AddListener(PlayVoice);

            onSelected.AddListener(UpdatePlayVoiceIconsVisibility);
            onDeselected.AddListener(UpdatePlayVoiceIconsVisibility);
            onConfirmed.AddListener(PlayVoice);

        }

        private void OnEnable()
        {
            UpdatePlayVoiceIconsVisibility();
            InputManager.onDeviceTypeChanged += UpdatePlayVoiceIconsVisibility;
        }

        private void OnDisable()
        {
            UpdatePlayVoiceIconsVisibility();
            InputManager.onDeviceTypeChanged -= UpdatePlayVoiceIconsVisibility;
        }

        private void UpdatePlayVoiceIconsVisibility(InputDeviceType deviceType)
        {
            UpdatePlayVoiceIconsVisibility();
        }

        private void UpdatePlayVoiceIconsVisibility()
        {
            bool voiceAvailable = voiceClip != null;
            bool touchStyle = InputManager.CurrentDeviceType is InputDeviceType.Touchscreen or InputDeviceType.KeyboardAndMouse;

            bool isFirst = false;
            bool isLast = false;
            if (selected && Panel.Items.Count > 0)
            {
                isFirst = Panel.Items[0] == this;
                isLast = Panel.Items[^1] == this;
            }

            touchStylePlayVoiceButton.gameObject.SetActive(voiceAvailable && touchStyle);
            gamepadStyleUpArrow.enabled = !touchStyle && selected && !isFirst;
            gamepadStyleDownArrow.enabled = !touchStyle && selected && !isLast;
            gamepadStyleVoicePlayButtonText.enabled = voiceAvailable && !touchStyle && selected;
            gamepadStyleVoiceIcon.enabled = voiceAvailable && !touchStyle && selected;
        }

        public void PlayVoice()
        {
            Panel.PlayVoice(voiceClip);
        }

        // disable pointer enter/click events as we use customized buttons for touchscreen and mouse
        public override void OnPointerClick(PointerEventData eventData)
        {
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
        }
    }
}