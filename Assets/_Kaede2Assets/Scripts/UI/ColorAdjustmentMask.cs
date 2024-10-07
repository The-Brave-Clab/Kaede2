using System;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class ColorAdjustmentMask : MonoBehaviour
    {
        private static readonly string ShaderName = "UI/UI Color Adjustment";

        private static readonly int HueAdjustment = Shader.PropertyToID("_HueAdjustment");
        private static readonly int SaturationAdjustment = Shader.PropertyToID("_SaturationAdjustment");
        private static readonly int LightnessAdjustment = Shader.PropertyToID("_LightnessAdjustment");
        private static readonly int ColorTintAdjustment = Shader.PropertyToID("_ColorTint");

        [SerializeField]
        private Image image;

        private void Awake()
        {
            if (!CheckImage())
            {
                this.LogError($"Image {image} does not have a material with shader {ShaderName}");
                gameObject.SetActive(false);
            }

            image.material = new Material(image.material);
        }

        public float Hue
        {
            get => image.material.GetFloat(HueAdjustment);
            set => image.material.SetFloat(HueAdjustment, Mathf.Clamp(value, -1, 1));
        }

        public float Saturation
        {
            get => image.material.GetFloat(SaturationAdjustment);
            set => image.material.SetFloat(SaturationAdjustment, Mathf.Clamp(value, -1, 1));
        }

        public float Lightness
        {
            get => image.material.GetFloat(LightnessAdjustment);
            set => image.material.SetFloat(LightnessAdjustment, Mathf.Clamp(value, -1, 1));
        }

        public Color Color
        {
            get => image.material.GetColor(ColorTintAdjustment).NoAlpha();
            set => image.material.SetColor(ColorTintAdjustment, value.NoAlpha());
        }

        private bool CheckImage()
        {
            if (image == null) return false;
            if (image.material == null) return false;
            if (image.material.shader.name != ShaderName) return false;

            return true;
        }
    }
}