using System.Collections;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabVoiceButton : SelectableItem, IThemeChangeObserver
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Image circle;

        [SerializeField]
        private RemapRGB colorComponent;

        private CommonButtonColor highlightedColor;

        private Coroutine changeColorCoroutine;
        private Sequence changeColorSequence;

        protected override void Awake()
        {
            OnThemeChange(Theme.Current);

            onSelected.AddListener(() => ChangeColor(true));
            onDeselected.AddListener(() => ChangeColor(false));
            
            base.Awake();
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            highlightedColor = theme.CommonButtonColor;
            RefreshColorImmediate();
        }

        private (Color textColor, CommonButtonColor buttonColor) GetColors(bool selected)
        {
            return selected ?
                (Color.white, highlightedColor) :
                (new Color(0.1254902f, 0.1372549f, 0.145098f), CommonButtonColor.Deactivated);
        }

        private void SetColor(Color textColor, CommonButtonColor buttonColor)
        {
            text.color = textColor;
            icon.color = textColor;
            circle.color = buttonColor.outline;
            colorComponent.targetColorRed = buttonColor.surface;
            colorComponent.targetColorGreen = buttonColor.outline;
            colorComponent.targetColorBlue = buttonColor.shadow;
        }

        private void RefreshColorImmediate()
        {
            var (textColor, buttonColor) = GetColors(selected);
            SetColor(textColor, buttonColor);
        }

        private void ChangeColor(bool selected)
        {
            if (changeColorCoroutine != null)
            {
                StopCoroutine(changeColorCoroutine);
                changeColorSequence.Kill();
                changeColorCoroutine = null;
                changeColorSequence = null;
            }

            changeColorCoroutine = StartCoroutine(ChangeColorCoroutine(selected));
        }

        private IEnumerator ChangeColorCoroutine(bool selected)
        {
            var endColors = GetColors(selected);

            Color startTextColor = text.color;
            CommonButtonColor startButtonColor = new()
            {
                surface = colorComponent.targetColorRed,
                outline = colorComponent.targetColorGreen,
                shadow = colorComponent.targetColorBlue
            };

            changeColorSequence = DOTween.Sequence();

            changeColorSequence.Append(DOVirtual.Float(0, 1, 0.1f, t =>
            {
                Color textColor = Color.Lerp(startTextColor, endColors.textColor, t);
                CommonButtonColor buttonColor = CommonButtonColor.Lerp(startButtonColor, endColors.buttonColor, t);
                SetColor(textColor, buttonColor);
            }));

            yield return changeColorSequence.WaitForCompletion();

            SetColor(endColors.textColor, endColors.buttonColor);

            changeColorSequence = null;
            changeColorCoroutine = null;
        }
    }
}
