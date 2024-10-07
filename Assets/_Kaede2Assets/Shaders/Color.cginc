#include "UnityCG.cginc"

// -------------------------------------------
// from com.unity.render-pipeline.core

// Hue, Saturation, Value
// Ranges:
//  Hue [0.0, 1.0]
//  Sat [0.0, 1.0]
//  Lum [0.0, HALF_MAX]
fixed3 RgbToHsv(fixed3 c)
{
    const fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
    fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    const float e = 1.0e-4;
    return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

fixed3 HsvToRgb(fixed3 c)
{
    const fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float RotateHue(float value, float low, float hi)
{
    return (value < low)
            ? value + hi
            : (value > hi)
                ? value - hi
                : value;
}
// -------------------------------------------

fixed3 HsvToHsl(fixed3 hsv)
{
    fixed3 hsl;
    hsl.x = hsv.x;
    hsl.z = hsv.z * 0.5 * (2.0 - hsv.y);
    float denominator = 1.0 - abs(2.0 * hsl.z - 1.0);
    if (denominator != 0.0)
    {
        hsl.y = hsv.z * hsv.y / denominator;
    }
    else
    {
        hsl.y = 0.0;
    }
    return hsl;
}

fixed3 HslToHsv(fixed3 hsl)
{
    fixed3 hsv;
    hsv.x = hsl.x;
    hsv.z = hsl.z + hsl.y * (0.5 - abs(hsl.z - 0.5));
    if (hsv.z == 0.0)
    {
        hsv.y = 0.0;
    }
    else
    {
        hsv.y = 2.0 * (1.0 - hsl.z / hsv.z);
    }
    return hsv;
}

fixed3 RgbToHsl(fixed3 rgb)
{
    return HsvToHsl(RgbToHsv(rgb));
}

fixed3 HslToRgb(fixed3 hsl)
{
    return HsvToRgb(HslToHsv(hsl));
}


fixed3 CalculateHSVAdjustment(fixed3 referenceColor, fixed3 targetColor)
{
    const fixed3 hsv1 = RgbToHsv(referenceColor);
    const fixed3 hsv2 = RgbToHsv(targetColor);

    fixed3 hsv = hsv2 - hsv1;

    return hsv;
}

fixed4 AdjustHSV(fixed4 rgba, fixed3 hsvAdjustment)
{
    fixed3 hsv = RgbToHsv(rgba.rgb);
    hsv.x = RotateHue(hsv.x + hsvAdjustment.x, 0, 1);
    hsv.y = clamp(hsv.y + hsvAdjustment.y, 0, 1);
    hsv.z = clamp(hsv.z + hsvAdjustment.z, 0, 1);
    return fixed4(HsvToRgb(hsv), rgba.a);
}
