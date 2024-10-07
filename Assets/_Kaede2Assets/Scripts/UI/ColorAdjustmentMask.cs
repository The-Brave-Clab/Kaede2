using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class ColorAdjustmentMask : CustomUIMaterial
    {
        private static readonly int HueAdjustment = Shader.PropertyToID("_HueAdjustment");
        private static readonly int SaturationAdjustment = Shader.PropertyToID("_SaturationAdjustment");
        private static readonly int LightnessAdjustment = Shader.PropertyToID("_LightnessAdjustment");
        private static readonly int ColorTintAdjustment = Shader.PropertyToID("_ColorTint");

        protected override string shaderName => "UI/UI Color Adjustment";

        [SerializeField]
        [Range(-1, 1)]
        private float hue;

        [SerializeField]
        [Range(-1, 1)]
        private float saturation;

        [SerializeField]
        [Range(-1, 1)]
        private float lightness;

        [SerializeField]
        private Color color;

        private bool needUpdate;

        protected override void Awake()
        {
            base.Awake();
            needUpdate = true;
        }

        protected override bool NeedUpdate()
        {
            bool result = needUpdate;
            needUpdate = false;
            return result;
        }

        protected override void UpdateMaterial(Material material, Material materialForRendering)
        {
            if (material != null)
            {
                material.SetFloat(HueAdjustment, hue);
                material.SetFloat(SaturationAdjustment, saturation);
                material.SetFloat(LightnessAdjustment, lightness);
                material.SetColor(ColorTintAdjustment, color.NoAlpha());
            }

            if (materialForRendering != null)
            {
                materialForRendering.SetFloat(HueAdjustment, hue);
                materialForRendering.SetFloat(SaturationAdjustment, saturation);
                materialForRendering.SetFloat(LightnessAdjustment, lightness);
                materialForRendering.SetColor(ColorTintAdjustment, color.NoAlpha());
            }
        }

        public float Hue
        {
            get => hue;
            set
            {
                hue = Mathf.Clamp(value, -1, 1);
                needUpdate = true;
            }
        }

        public float Saturation
        {
            get => saturation;
            set
            {
                saturation = Mathf.Clamp(value, -1, 1);
                needUpdate = true;
            }
        }

        public float Lightness
        {
            get => lightness;
            set
            {
                lightness = Mathf.Clamp(value, -1, 1);
                needUpdate = true;
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value.NoAlpha();
                needUpdate = true;
            }
        }
    }
}