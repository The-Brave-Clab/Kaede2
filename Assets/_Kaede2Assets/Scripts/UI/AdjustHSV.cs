using System;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class AdjustHSV : CustomUIMaterial
    {
        public Adjustment adjustment = new()
        {
            hsvAdjustment = Vector3.zero,
            referenceColor = Color.red
        };

        private Vector3 lastHSVAdjustment;

        private static readonly int ReferenceColor = Shader.PropertyToID("_ReferenceColor");
        private static readonly int TargetColor = Shader.PropertyToID("_TargetColor");

        [Serializable]
        public struct Adjustment
        {
            [ColorUsage(false, false)]
            public Color referenceColor;

            public Vector3 hsvAdjustment;
        }

        protected override string shaderName => "UI/HSV Adjustable";

        protected override void Awake()
        {
            base.Awake();

            material.SetColor(ReferenceColor, adjustment.referenceColor);
            material.SetColor(TargetColor, CalculateTargetColor(adjustment.referenceColor, adjustment.hsvAdjustment));

            lastHSVAdjustment = new Vector3(Single.NaN, Single.NaN, Single.NaN);
        }

        protected override void UpdateMaterial(Material material, Material materialForRendering)
        {
            if (adjustment.hsvAdjustment == lastHSVAdjustment) return;

            var targetColor = CalculateTargetColor(adjustment.referenceColor, adjustment.hsvAdjustment);
            material.SetColor(ReferenceColor, adjustment.referenceColor);
            material.SetColor(TargetColor, targetColor);
            materialForRendering.SetColor(ReferenceColor, adjustment.referenceColor);
            materialForRendering.SetColor(TargetColor, targetColor);
            lastHSVAdjustment = adjustment.hsvAdjustment;
        }

        public static Color CalculateTargetColor(Color referenceColor, Vector3 hsvAdjustment)
        {
            Color.RGBToHSV(referenceColor, out var h, out var s, out var v);
            h += hsvAdjustment.x;
            s += hsvAdjustment.y;
            v += hsvAdjustment.z;

            h = Mathf.Repeat(h, 1);

            Color result = Color.HSVToRGB(h, s, v);
            result.a = referenceColor.a;
            return result;
        }
    }
}