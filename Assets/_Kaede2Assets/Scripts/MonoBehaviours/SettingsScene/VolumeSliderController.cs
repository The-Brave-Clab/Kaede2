using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class VolumeSliderController : MonoBehaviour
    {
        private enum AudioType
        {
            Master,
            BGM,
            SE,
            Voice
        }

        [SerializeField]
        private SliderControl slider;

        [SerializeField]
        private TextMeshProUGUI valueText;

        [SerializeField]
        private Image volumeIcon;

        [SerializeField]
        private Sprite mutedIcon;

        [SerializeField]
        private Sprite onIcon;

        [SerializeField]
        private AudioType type;

        private void Awake()
        {
            float volume = type switch
            {
                AudioType.Master => GameSettings.AudioMasterVolume,
                AudioType.BGM => GameSettings.AudioBGMVolume,
                AudioType.SE => GameSettings.AudioSEVolume,
                AudioType.Voice => GameSettings.AudioVoiceVolume,
                _ => throw new ArgumentOutOfRangeException()
            };

            slider.SetValueDisplayOnly(volume);
            valueText.text = $"{Mathf.RoundToInt(volume * 10)}";
        }

        public void SliderValueChanged(float value)
        {
            int volume = Mathf.FloorToInt(value * 10);
            valueText.text = $"{volume}";
            volumeIcon.sprite = volume == 0 ? mutedIcon : onIcon;
        }

        public void SetVolume(float value)
        {
            int volume = Mathf.FloorToInt(value * 10);
            var volumeValue = volume / 10.0f;
            slider.Value = volumeValue;
            switch (type)
            {
                case AudioType.Master:
                    GameSettings.AudioMasterVolume = volumeValue;
                    break;
                case AudioType.BGM:
                    GameSettings.AudioBGMVolume = volumeValue;
                    break;
                case AudioType.SE:
                    GameSettings.AudioSEVolume = volumeValue;
                    break;
                case AudioType.Voice:
                    GameSettings.AudioVoiceVolume = volumeValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}