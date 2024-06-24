using System;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class RemapRGB : CustomUIMaterial
    {
        [ColorUsage(true, false)]
        public Color targetColorRed = Color.red;

        [ColorUsage(true, false)]
        public Color targetColorGreen = Color.green;

        [ColorUsage(true, false)]
        public Color targetColorBlue = Color.blue;

        private Color lastTargetColorRed;
        private Color lastTargetColorGreen;
        private Color lastTargetColorBlue;

        private static readonly int TargetColorR = Shader.PropertyToID("_RedTargetColor");
        private static readonly int TargetColorG = Shader.PropertyToID("_GreenTargetColor");
        private static readonly int TargetColorB = Shader.PropertyToID("_BlueTargetColor");

        protected override string shaderName => "UI/UI Remap Color";

        protected override void Awake()
        {
            base.Awake();

            material.SetColor(TargetColorR, targetColorRed);
            material.SetColor(TargetColorG, targetColorGreen);
            material.SetColor(TargetColorB, targetColorBlue);

            lastTargetColorRed = targetColorRed;
            lastTargetColorGreen = targetColorGreen;
            lastTargetColorBlue = targetColorBlue;
        }

        protected override bool NeedUpdate()
        {
            return targetColorRed != lastTargetColorRed ||
                   targetColorGreen != lastTargetColorGreen ||
                   targetColorBlue != lastTargetColorBlue;
        }

        protected override void UpdateMaterial(Material material, Material materialForRendering)
        {
            material.SetColor(TargetColorR, targetColorRed);
            materialForRendering.SetColor(TargetColorR, targetColorRed);
            lastTargetColorRed = targetColorRed;

            material.SetColor(TargetColorG, targetColorGreen);
            materialForRendering.SetColor(TargetColorG, targetColorGreen);
            lastTargetColorGreen = targetColorGreen;

            material.SetColor(TargetColorB, targetColorBlue);
            materialForRendering.SetColor(TargetColorB, targetColorBlue);
            lastTargetColorBlue = targetColorBlue;
        }
    }
}