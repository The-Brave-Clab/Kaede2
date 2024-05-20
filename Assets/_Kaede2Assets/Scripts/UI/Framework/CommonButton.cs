using System;
using System.Collections;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kaede2.UI.Framework
{
    public class CommonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IThemeChangeObserver
    {
        [SerializeField]
        private RemapRGB colorComponent;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private bool interactable = true;

        private bool activated;

        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value) return;
                interactable = value;

                StopCurrentCoroutine();
                changeColorCoroutine = StartCoroutine(ChangeColorCoroutine());
            }
        }

        public UnityEvent onActivate;
        public UnityEvent onDeactivate;
        public UnityEvent onClick;

        private CommonButtonColor activatedColor;

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
            changeColorCoroutine = StartCoroutine(ChangeColorCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            activated = false;
            if (!Interactable) return;

            onDeactivate.Invoke();

            StopCurrentCoroutine();
            changeColorCoroutine = StartCoroutine(ChangeColorCoroutine());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable) return;

            onClick.Invoke();
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            activatedColor = theme.CommonButtonColor;

            var colors = GetColors(Interactable, activated, activatedColor);
            SetColor(colors.textColor, colors.buttonColor);
        }

        private static (Color textColor, CommonButtonColor buttonColor) GetColors(bool interactable, bool activated, CommonButtonColor activatedColor)
        {
            if (!interactable)
                return (new Color(0.8705882f, 0.8705882f, 0.8705882f), CommonButtonColor.Disabled);
            if (!activated)
                return (new Color(0.1254902f, 0.1372549f, 0.145098f), CommonButtonColor.Deactivated);
            return (Color.white, activatedColor);
        }

        private void SetColor(Color textColor, CommonButtonColor buttonColor)
        {
            text.color = textColor;
            colorComponent.targetColorRed = buttonColor.surface;
            colorComponent.targetColorGreen = buttonColor.outline;
            colorComponent.targetColorBlue = buttonColor.shadow;
        }

        private void StopCurrentCoroutine()
        {
            if (changeColorCoroutine != null)
            {
                StopCoroutine(changeColorCoroutine);
                changeColorSequence.Kill();
                changeColorCoroutine = null;
                changeColorSequence = null;
            }
        }

        private IEnumerator ChangeColorCoroutine()
        {
            var endColors = GetColors(Interactable, activated, activatedColor);

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