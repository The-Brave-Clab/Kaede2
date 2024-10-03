using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class OptionButton : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private RemapRGB remapRGB;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private CommonButtonColor nonHighlightColor;

        [SerializeField]
        private Color nonHighlightTextColor;

        [SerializeField]
        private SelectableItem selectableItem;

        public SelectableItem SelectableItem => selectableItem;

        private CommonButtonColor highlightColor;
        private Color highlightTextOutlineColor;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
            nonHighlightColor = nonHighlightColor.NonTransparent();

            selectableItem.onSelected.AddListener(() => UpdateColor(true));
            selectableItem.onDeselected.AddListener(() => UpdateColor(false));
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightColor = theme.OptionColor.NonTransparent();
            highlightTextOutlineColor = theme.MainTextRim;

            UpdateColor();
        }

        public void UpdateColor()
        {
            UpdateColor(selectableItem.selected);
        }

        private void UpdateColor(bool selected)
        {
            if (selected)
            {
                remapRGB.targetColorRed = highlightColor.surface;
                remapRGB.targetColorGreen = highlightColor.outline;
                remapRGB.targetColorBlue = highlightColor.shadow;

                text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, highlightTextOutlineColor);
                text.color = Color.white;
            }
            else
            {
                remapRGB.targetColorRed = nonHighlightColor.surface;
                remapRGB.targetColorGreen = nonHighlightColor.outline;
                remapRGB.targetColorBlue = nonHighlightColor.shadow;

                text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.clear);
                text.color = nonHighlightTextColor;
            }
        }
    }
}