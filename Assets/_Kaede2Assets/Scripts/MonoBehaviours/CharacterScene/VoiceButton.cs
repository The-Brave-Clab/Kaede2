using Kaede2.Audio;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2
{
    public class VoiceButton : SelectableItem, IThemeChangeObserver
    {
        [SerializeField]
        private Color nonHighlightIconColor;

        [SerializeField]
        private Color disabledCircleColor;

        [SerializeField]
        private Color disabledIconColor;

        [SerializeField]
        private Image circle;

        [SerializeField]
        private Image icon;

        private Color highlightCircleColor;

        private string voiceName;

        protected override void Awake()
        {
            base.Awake()
;
            OnThemeChange(Theme.Current);

            onSelected.AddListener(UpdateColor);
            onDeselected.AddListener(UpdateColor);
            onConfirmed.AddListener(PlayVoice);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightCircleColor = theme.VoiceButtonCircle;
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (!Valid())
            {
                circle.color = disabledCircleColor;
                icon.color = disabledIconColor;
            }
            else if (selected)
            {
                circle.color = highlightCircleColor;
                icon.color = Color.white;
            }
            else
            {
                circle.color = Color.white;
                icon.color = nonHighlightIconColor;
            }
        }

        public void SetVoice(string voiceName)
        {
            this.voiceName = voiceName;
            UpdateColor();
        }

        private void PlayVoice()
        {
            AudioManager.PlayVoice(voiceName, true);
        }

        public bool Valid()
        {
            return !string.IsNullOrEmpty(voiceName);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!Valid()) return;
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!Valid()) return;
            base.OnPointerClick(eventData);
        }
    }
}