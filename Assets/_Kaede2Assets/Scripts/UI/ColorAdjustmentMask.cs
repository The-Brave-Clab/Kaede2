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
        private static readonly int ValueAdjustment = Shader.PropertyToID("_ValueAdjustment");

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

        public float Value
        {
            get => image.material.GetFloat(ValueAdjustment);
            set => image.material.SetFloat(ValueAdjustment, Mathf.Clamp(value, -1, 1));
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