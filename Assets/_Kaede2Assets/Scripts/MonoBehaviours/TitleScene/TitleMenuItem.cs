using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class TitleMenuItem : SelectableItem, IThemeChangeObserver
    {
        [SerializeField]
        private Image backgroundTop;

        [SerializeField]
        private Image backgroundBottom;

        [SerializeField]
        private Image overlay;

        [SerializeField]
        private TextMeshProUGUI text;

        private Color highlightTop;
        private Color highlightBottom;
        private Color textRim;

        protected override void Awake()
        {
            base.Awake();

            OnThemeChange(Theme.Current);

            onSelected.AddListener(UpdateButtonAppearance);
            onDeselected.AddListener(UpdateButtonAppearance);
        }

        public void UpdateButtonAppearance()
        {
            overlay.enabled = selected;
            backgroundTop.color = selected ? highlightTop : new Color(1, 1, 1, 0.902f);
            backgroundBottom.color = selected ? highlightBottom : new Color(0.808f, 0.812f, 0.808f, 0.988f);
            text.color = selected ? Color.white : Color.black;
            // text.outlineColor = selected ? textRim : Color.black;
            // text.outlineWidth = selected ? 1 : 0;
            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, selected ? textRim : Color.black);
            text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, selected ? 1 : 0);
            text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, selected ? 1 : 0);
            text.UpdateFontAsset();
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightTop = theme.MainThemeColor;
            highlightBottom = theme.MenuButtonHighlightBottom;
            textRim = theme.MainTextRim;

            UpdateButtonAppearance();
        }
    }
}