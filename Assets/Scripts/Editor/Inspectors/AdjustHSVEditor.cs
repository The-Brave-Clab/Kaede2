using System;
using Kaede2.UI;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(AdjustHSV))]
    public class AdjustHSVEditor : UnityEditor.Editor
    {
        private SerializedProperty hsvAdjustment;
        private SerializedProperty referenceColor;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();
            var script = serializedObject.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(script, true);
            }

            hsvAdjustment = serializedObject.FindProperty("hsvAdjustment");
            referenceColor = serializedObject.FindProperty("referenceColor");

            DrawHSVAdjustment();

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static (float h, float s, float v) RGBToHSV(Color rgb)
        {
            var r = rgb.r;
            var g = rgb.g;
            var b = rgb.b;
            var minRGB = Mathf.Min(Mathf.Min(r, g), b);
            var maxRGB = Mathf.Max(Mathf.Max(r, g), b);
            var deltaRGB = maxRGB - minRGB;

            float h = 0;
            float s = 0;
            var v = maxRGB;

            if (deltaRGB == 0) return (h, s, v);

            s = deltaRGB / maxRGB;
            var deltaR = (((maxRGB - r) / 6) + (deltaRGB / 2)) / deltaRGB;
            var deltaG = (((maxRGB - g) / 6) + (deltaRGB / 2)) / deltaRGB;
            var deltaB = (((maxRGB - b) / 6) + (deltaRGB / 2)) / deltaRGB;

            if (Mathf.Abs(r - maxRGB) < Mathf.Epsilon) h = deltaB - deltaG;
            else if (Mathf.Abs(g - maxRGB) < Mathf.Epsilon) h = (1.0f / 3.0f) + deltaR - deltaB;
            else if (Mathf.Abs(b - maxRGB) < Mathf.Epsilon) h = (2.0f / 3.0f) + deltaG - deltaR;

            if (h < 0) h += 1;
            if (h > 1) h -= 1;

            return (h, s, v);
        }

        private static Color HSVToRGB(float h, float s, float v)
        {
            float r = 0;
            float g = 0;
            float b = 0;

            if (s == 0)
            {
                r = g = b = v;
            }
            else
            {
                var i = (uint)(h * 6);
                var f = (h * 6) - i;
                var p = v * (1 - s);
                var q = v * (1 - f * s);
                var t = v * (1 - (1 - f) * s);

                switch (i % 6) {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                }
            }

            return new Color(r, g, b, 1);
        }

        private static Color CalculateTargetColor(Color referenceColor, Vector3 hsvAdjustment)
        {
            var (h, s, v) = RGBToHSV(referenceColor);
            h += hsvAdjustment.x;
            s += hsvAdjustment.y;
            v += hsvAdjustment.z;

            while (h > 1) h -= 1;
            while (h < 0) h += 1;

            s = Mathf.Clamp01(s);
            v = Mathf.Clamp01(v);

            return HSVToRGB(h, s, v);
        }

        private static Vector3 CalculateHSVAdjustment(Color referenceColor, Color targetColor)
        {
            var (h1, s1, v1) = RGBToHSV(referenceColor);
            var (h2, s2, v2) = RGBToHSV(targetColor);

            var h = h2 - h1;
            var s = s2 - s1;
            var v = v2 - v1;

            while (h < -0.5f) h += 1;
            while (h > 0.5f) h -= 1;

            return new Vector3(h, s, v);
        }

        private void DrawHSVAdjustment()
        {
            Color currentReferenceColor = referenceColor.colorValue;
            Vector3 currentHSVAdjustment = hsvAdjustment.vector3Value;
            Color currentTargetColor = CalculateTargetColor(currentReferenceColor, currentHSVAdjustment);

            // reference color
            EditorGUI.BeginChangeCheck();
            currentReferenceColor = EditorGUILayout.ColorField("Reference Color", currentReferenceColor);
            if (EditorGUI.EndChangeCheck())
            {
                currentHSVAdjustment = CalculateHSVAdjustment(currentReferenceColor, currentTargetColor);
                referenceColor.colorValue = currentReferenceColor;
                hsvAdjustment.vector3Value = currentHSVAdjustment;
                serializedObject.ApplyModifiedProperties();
            }

            // target color
            // this is not a serialized property, so we need to draw it manually
            EditorGUI.BeginChangeCheck();
            currentTargetColor = EditorGUILayout.ColorField("Target Color", currentTargetColor);
            if (EditorGUI.EndChangeCheck())
            {
                currentHSVAdjustment = CalculateHSVAdjustment(currentReferenceColor, currentTargetColor);
                hsvAdjustment.vector3Value = currentHSVAdjustment;
                serializedObject.ApplyModifiedProperties();
            }

            // hsv adjustment
            // if modified, change the target color
            EditorGUI.BeginChangeCheck();
            currentHSVAdjustment = EditorGUILayout.Vector3Field("HSV Adjustment", currentHSVAdjustment);
            if (EditorGUI.EndChangeCheck())
            {
                // limit h value
                currentHSVAdjustment.x = Mathf.Clamp(currentHSVAdjustment.x, -0.5f, 0.5f);
                // target color will be calculated next frame
                hsvAdjustment.vector3Value = currentHSVAdjustment;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}