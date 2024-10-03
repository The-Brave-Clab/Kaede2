using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class VoiceButton : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private SelectableItem selectableItem;

        [SerializeField]
        private Color nonHighlightIconColor;

        [SerializeField]
        private Image circle;

        [SerializeField]
        private Image icon;

        private Color highlightCircleColor;

        private string voiceName;

        private void Awake()
        {
            OnThemeChange(Theme.Current);

            selectableItem.onSelected.AddListener(() => UpdateColor(true));
            selectableItem.onDeselected.AddListener(() => UpdateColor(false));
            selectableItem.onConfirmed.AddListener(PlayVoice);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightCircleColor = theme.VoiceButtonCircle;
            UpdateColor(selectableItem.selected);
        }

        private void UpdateColor(bool selected)
        {
            circle.color = selected ? highlightCircleColor : Color.white;
            icon.color = selected ? Color.white : nonHighlightIconColor;
        }

        public void SetVoice(string voiceName)
        {
            this.voiceName = voiceName;
        }

        private void PlayVoice()
        {
            // TODO
            this.Log($"Play voice: {voiceName}");
        }
    }
}