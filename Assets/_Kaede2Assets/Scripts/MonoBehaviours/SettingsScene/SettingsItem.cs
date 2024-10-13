using DG.Tweening;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Kaede2
{
    public class SettingsItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IThemeChangeObserver
    {
        [SerializeField]
        private SettingsControl control;

        [SerializeField]
        private RemapRGB colorComponent;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        [TextArea]
        private string infoText;

        public UnityEvent onPointerEnter;

        private CommonButtonColor activeColor;

        private bool activated;

        private bool shouldActivate;
        private static SettingsItem currentPointerDown = null;
        private static SettingsItem currentPointerOver = null;

        private Coroutine changeColorCoroutine;
        private Sequence changeColorSequence;

        public SettingsControl Control => control;

        public string InfoText
        {
            get => infoText;
            set
            {
                if (string.Equals(infoText, value)) return;
                infoText = value;
                if (activated) InfoBar.Text = infoText;
            }
        }

        private void Awake()
        {
            activated = false;
            shouldActivate = false;
            OnThemeChange(Theme.Current);

            OnFontChange();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            shouldActivate = true;
            if (currentPointerOver != null && currentPointerOver != this)
            {
                currentPointerOver.shouldActivate = false;
                currentPointerOver.ChangeActiveState(false);
            }
            currentPointerOver = this;
            ChangeActiveState(true);
            onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (InputManager.CurrentDeviceType == InputDeviceType.Touchscreen) return;
            shouldActivate = false;
            currentPointerOver = null;
            ChangeActiveState(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            currentPointerDown = this;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (currentPointerDown == this)
                currentPointerDown = null;
            ChangeActiveState(shouldActivate);
            if (currentPointerOver != null)
                currentPointerOver.ChangeActiveState(currentPointerOver.shouldActivate);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            activeColor = theme.SettingsItemColor;

            UpdateColor();
        }

        private void UpdateColor()
        {
            CommonButtonColor color = activated ? activeColor : CommonButtonColor.Deactivated;

            colorComponent.targetColorRed = color.surface;
            colorComponent.targetColorGreen = color.outline;
            colorComponent.targetColorBlue = color.shadow;
        }

        public void OnFontChange()
        {
            // this does not only set the color, but also instantiates the material so that we can safely set properties for materialForRendering
            var underlayKeyword = new LocalKeyword(text.fontMaterial.shader, ShaderUtilities.Keyword_Underlay);
            text.fontMaterial.SetKeyword(underlayKeyword, true);
            text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, activated ? Color.black : Color.clear);
            text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.25f);
            text.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1f);
            text.UpdateFontAsset();
        }

        private void ChangeActiveState(bool active)
        {
            if (currentPointerDown != null) return;
            if (active == activated) return;

            activated = active;

            InfoBar.Text = activated ? infoText : string.Empty;

            if (changeColorCoroutine != null)
            {
                CoroutineProxy.Stop(changeColorCoroutine);
                changeColorSequence.Kill();
                changeColorCoroutine = null;
                changeColorSequence = null;
            }

            changeColorCoroutine = CoroutineProxy.Start(ChangeColorCoroutine(active));
        }

        private System.Collections.IEnumerator ChangeColorCoroutine(bool active)
        {
            CommonButtonColor startColor = new()
            {
                surface = colorComponent.targetColorRed,
                outline = colorComponent.targetColorGreen,
                shadow = colorComponent.targetColorBlue
            };
            CommonButtonColor endColor = active ? activeColor : CommonButtonColor.Deactivated;

            Color startTextColor = text.color;
            Color endTextColor = active ? Color.white : Color.black;

            Color startUnderlayColor = text.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor);
            Color endUnderlayColor = active ? Color.black : Color.clear;

            changeColorSequence = DOTween.Sequence();
            changeColorSequence.Append(DOVirtual.Float(0, 1, 0.1f, t =>
            {
                if (text == null) return;
                if (colorComponent == null) return;
                if (text.fontMaterial == null) return;
                if (text.materialForRendering == null) return;

                CommonButtonColor color = CommonButtonColor.Lerp(startColor, endColor, t);
                colorComponent.targetColorRed = color.surface;
                colorComponent.targetColorGreen = color.outline;
                colorComponent.targetColorBlue = color.shadow;

                text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.Lerp(startUnderlayColor, endUnderlayColor, t));
                text.materialForRendering.SetColor(ShaderUtilities.ID_UnderlayColor, Color.Lerp(startUnderlayColor, endUnderlayColor, t));
                text.color = Color.Lerp(startTextColor, endTextColor, t);

                text.UpdateFontAsset();
            }));

            yield return changeColorSequence.WaitForCompletion();

            colorComponent.targetColorRed = endColor.surface;
            colorComponent.targetColorGreen = endColor.outline;
            colorComponent.targetColorBlue = endColor.shadow;

            text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, endUnderlayColor);
            text.materialForRendering.SetColor(ShaderUtilities.ID_UnderlayColor, endUnderlayColor);
            text.color = endTextColor;

            text.UpdateFontAsset();

            changeColorCoroutine = null;
            changeColorSequence = null;
        }
    }
}