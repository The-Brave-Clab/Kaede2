using System;
using System.Collections.Generic;
using Kaede2.UI;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomPropertyDrawer(typeof(AdjustHSV.Adjustment))]
    public class HSVAdjustmentDrawer : PropertyDrawer
    {
        private SerializedProperty hsvAdjustment;
        private SerializedProperty referenceColor;

        private Dictionary<string, bool> foldouts = new();

        private bool GetFoldout(SerializedProperty property)
        {
            foldouts.TryAdd(property.propertyPath, false);
            return foldouts[property.propertyPath];
        }

        private void SetFoldout(SerializedProperty property, bool value)
        {
            foldouts[property.propertyPath] = value;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // we are drawing 3 fields and a foldout
            // when foldout is closed, we still draw the target color field
            return EditorGUIUtility.singleLineHeight * (GetFoldout(property) ? 4 : 2);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            hsvAdjustment = property.FindPropertyRelative("hsvAdjustment");
            referenceColor = property.FindPropertyRelative("referenceColor");

            position.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUIUtility.hierarchyMode = true;
            SetFoldout(property, EditorGUI.Foldout(position, GetFoldout(property), label, true));
            EditorGUIUtility.hierarchyMode = false;
            EditorGUI.indentLevel += 1;
            DrawHSVAdjustment(position, GetFoldout(property));
            EditorGUI.indentLevel -= 1;

            EditorGUI.EndProperty();
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

        private void DrawHSVAdjustment(Rect position, bool foldout)
        {
            Color currentReferenceColor = referenceColor.colorValue;
            Vector3 currentHSVAdjustment = hsvAdjustment.vector3Value;
            Color currentTargetColor = AdjustHSV.CalculateTargetColor(currentReferenceColor, currentHSVAdjustment);

            // reference color
            if (foldout)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                currentReferenceColor = EditorGUI.ColorField(position, new("Reference Color"), currentReferenceColor, true, false, false);
                currentReferenceColor.a = 1;
                if (EditorGUI.EndChangeCheck())
                {
                    currentHSVAdjustment = CalculateHSVAdjustment(currentReferenceColor, currentTargetColor);
                    referenceColor.colorValue = currentReferenceColor;
                    hsvAdjustment.vector3Value = currentHSVAdjustment;
                }
            }

            // target color
            // this is not a serialized property, so we need to draw it manually
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            currentTargetColor = EditorGUI.ColorField(position, new("Target Color"), currentTargetColor, true, false, false);
            currentTargetColor.a = 1;
            if (EditorGUI.EndChangeCheck())
            {
                currentHSVAdjustment = CalculateHSVAdjustment(currentReferenceColor, currentTargetColor);
                hsvAdjustment.vector3Value = currentHSVAdjustment;
            }

            // hsv adjustment
            // if modified, change the target color
            if (foldout)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                currentHSVAdjustment = EditorGUI.Vector3Field(position, "HSV Adjustment", currentHSVAdjustment);
                if (EditorGUI.EndChangeCheck())
                {
                    // limit h value
                    currentHSVAdjustment.x = Mathf.Repeat(currentHSVAdjustment.x, 1);
                    // target color will be calculated next frame
                    hsvAdjustment.vector3Value = currentHSVAdjustment;
                }
            }
        }
    }
}