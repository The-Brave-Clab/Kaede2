using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class OptionButton : SelectableItem, IThemeChangeObserver
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
        private CommonButtonColor disabledColor;

        [SerializeField]
        private Color disabledTextColor;

        [SerializeField]
        private bool interactable = true;

        private CommonButtonColor highlightColor;
        private Color highlightTextOutlineColor;

        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                UpdateColor();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            OnThemeChange(Theme.Current);
            nonHighlightColor = nonHighlightColor.NonTransparent();
            disabledColor = disabledColor.NonTransparent();

            onSelected.AddListener(() => UpdateColor(true));
            onDeselected.AddListener(() => UpdateColor(false));
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightColor = theme.OptionColor;
            highlightTextOutlineColor = theme.MainTextRim;

            UpdateColor();
        }

        public void UpdateColor()
        {
            UpdateColor(selected);
        }

        private void UpdateColor(bool selected)
        {
            if (!interactable)
            {
                remapRGB.targetColorRed = disabledColor.surface;
                remapRGB.targetColorGreen = disabledColor.outline;
                remapRGB.targetColorBlue = disabledColor.shadow;

                text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.clear);
                text.color = disabledTextColor;
            }
            else if (selected)
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

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;
            base.OnPointerClick(eventData);
        }
    }
}