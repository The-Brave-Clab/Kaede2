using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Kaede2.UI
{
    public class BoxWindow : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private RemapRGB foregroundColor;

        [SerializeField]
        private RectTransform content;

        [SerializeField]
        private TextMeshProUGUI title;

        public string TitleText
        {
            get => title.text;
            set => title.text = value;
        }

        public TMP_FontAsset TitleFont
        {
            get => title.font;
            set => title.font = value;
        }

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            foregroundColor.targetColorRed = Color.white;
            foregroundColor.targetColorGreen = theme.WindowOutline;
            foregroundColor.targetColorBlue = Color.black;
        }
    }
}