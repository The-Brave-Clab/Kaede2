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
        private static Vector3 CalculateHSVAdjustment(Color referenceColor, Color targetColor)
        {
            Color.RGBToHSV(referenceColor, out var h1, out var s1, out var v1);
            Color.RGBToHSV(targetColor, out var h2, out var s2, out var v2);

            var h = h2 - h1;
            var s = s2 - s1;
            var v = v2 - v1;

            h = Mathf.Repeat(h, 1);

            return new Vector3(h, s, v);
        }

        private void DrawHSVAdjustment()
        {
            Color currentReferenceColor = referenceColor.colorValue;
            Vector3 currentHSVAdjustment = hsvAdjustment.vector3Value;
            Color currentTargetColor = AdjustHSV.CalculateTargetColor(currentReferenceColor, currentHSVAdjustment);

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
                currentHSVAdjustment.x = Mathf.Repeat(currentHSVAdjustment.x, 1);
                // target color will be calculated next frame
                hsvAdjustment.vector3Value = currentHSVAdjustment;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}