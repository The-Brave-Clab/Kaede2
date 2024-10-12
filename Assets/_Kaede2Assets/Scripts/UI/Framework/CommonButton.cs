using System;
using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kaede2.UI.Framework
{
    public class CommonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IThemeChangeObserver
    {
        public enum ButtonColorType
        {
            Color_3,
            Color_2
        }

        [SerializeField]
        private RemapRGB colorComponent;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private ButtonColorType colorType = ButtonColorType.Color_3;

        [SerializeField]
        private bool interactable = true;

        private bool activated;
        public bool Activated => activated;

        [SerializeField]
        private bool highlighted;

        public bool Highlighted
        {
            get => highlighted;
            set
            {
                if (highlighted == value) return;
                highlighted = value;

                StopCurrentCoroutine();
                changeColorCoroutine = CoroutineProxy.Start(ChangeColorCoroutine());
            }
        }

        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value) return;
                interactable = value;

                StopCurrentCoroutine();
                changeColorCoroutine = CoroutineProxy.Start(ChangeColorCoroutine());
            }
        }

        public UnityEvent onActivate;
        public UnityEvent onDeactivate;
        public UnityEvent onClick;

        private CommonButtonColor activatedColor;
        private CommonButtonColor highlightedColor;

        private Coroutine changeColorCoroutine;
        private Sequence changeColorSequence;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            activated = true;
            if (!Interactable) return;

            onActivate.Invoke();

            StopCurrentCoroutine();
            changeColorCoroutine = CoroutineProxy.Start(ChangeColorCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            activated = false;
            if (!Interactable) return;

            onDeactivate.Invoke();

            StopCurrentCoroutine();
            changeColorCoroutine = CoroutineProxy.Start(ChangeColorCoroutine());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable) return;

            onClick.Invoke();
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            activatedColor = theme.CommonButtonColor;
            highlightedColor = theme.SettingsItemColor;

            var colors = GetColors();
            SetColor(colors.textColor, colors.buttonColor);
        }

        public void SetImmediate(bool activated, bool highlighted, bool interactable)
        {
            this.activated = activated;
            this.highlighted = highlighted;
            this.interactable = interactable;

            var colors = GetColors();
            SetColor(colors.textColor, colors.buttonColor);
        }

        private (Color textColor, CommonButtonColor buttonColor) GetColors()
        {
            if (!interactable)
                return (new Color(0.8705882f, 0.8705882f, 0.8705882f), CommonButtonColor.Disabled);
            if (activated)
                return (Color.white, activatedColor);
            if (highlighted)
                return (Color.black, highlightedColor);
            return (new Color(0.1254902f, 0.1372549f, 0.145098f), CommonButtonColor.Deactivated);
        }

        private void SetColor(Color textColor, CommonButtonColor buttonColor)
        {
            text.color = textColor;
            switch (colorType)
            {
                case ButtonColorType.Color_3:
                    colorComponent.targetColorRed = buttonColor.surface;
                    colorComponent.targetColorGreen = buttonColor.outline;
                    colorComponent.targetColorBlue = buttonColor.shadow;
                    break;
                case ButtonColorType.Color_2:
                    colorComponent.targetColorRed = buttonColor.surface;
                    colorComponent.targetColorGreen = buttonColor.shadow;
                    colorComponent.targetColorBlue = buttonColor.outline;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StopCurrentCoroutine()
        {
            if (changeColorCoroutine != null)
            {
                CoroutineProxy.Stop(changeColorCoroutine);
                changeColorSequence.Kill();
                changeColorCoroutine = null;
                changeColorSequence = null;
            }
        }

        private IEnumerator ChangeColorCoroutine()
        {
            var endColors = GetColors();

            Color startTextColor = text.color;
            CommonButtonColor startButtonColor = colorType switch
            {
                ButtonColorType.Color_3 => new CommonButtonColor
                {
                    surface = colorComponent.targetColorRed,
                    outline = colorComponent.targetColorGreen,
                    shadow = colorComponent.targetColorBlue
                },
                ButtonColorType.Color_2 => new CommonButtonColor
                {
                    surface = colorComponent.targetColorRed,
                    outline = colorComponent.targetColorBlue,
                    shadow = colorComponent.targetColorGreen
                },
                _ => throw new ArgumentOutOfRangeException()
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