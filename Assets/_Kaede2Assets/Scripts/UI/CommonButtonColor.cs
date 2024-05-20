using System;
using UnityEngine;

namespace Kaede2.UI
{
    [Serializable]
    public struct CommonButtonColor
    {
        [ColorUsage(false, false)]
        public Color surface;

        [ColorUsage(false, false)]
        public Color outline;

        [ColorUsage(false, false)]
        public Color shadow;

        public static CommonButtonColor Lerp(CommonButtonColor a, CommonButtonColor b, float t)
        {
            return new CommonButtonColor
            {
                surface = Color.Lerp(a.surface, b.surface, t),
                outline = Color.Lerp(a.outline, b.outline, t),
                shadow = Color.Lerp(a.shadow, b.shadow, t)
            };
        }

        public static CommonButtonColor Deactivated => new CommonButtonColor
        {
            surface = new Color(0.9294118f, 0.9294118f, 0.9294118f),
            outline = new Color(0.7411765f, 0.7568628f, 0.7803922f),
            shadow = new Color(0.01176471f, 0.01176471f, 0.01176471f)
        };

        public static CommonButtonColor Disabled => new CommonButtonColor
        {
            surface = new Color(0.627451f, 0.627451f, 0.627451f),
            outline = new Color(0.5686275f, 0.572549f, 0.5803922f),
            shadow = new Color(0.003921569f, 0.003921569f, 0.003921569f)
        };

        public CommonButtonColor NonTransparent()
        {
            return new CommonButtonColor
            {
                surface = new Color(surface.r, surface.g, surface.b, 1.0f),
                outline = new Color(outline.r, outline.g, outline.b, 1.0f),
                shadow = new Color(shadow.r, shadow.g, shadow.b, 1.0f)
            };
        }
    }
}