using System;
using System.Globalization;
using Kaede2.Localization;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomPropertyDrawer(typeof(SerializableCultureInfo))]
    public class SerializableCultureInfoDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var cultureNameProperty = property.FindPropertyRelative("cultureName");

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            var newCultureName = EditorGUI.TextField(position, label, cultureNameProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    var newCultureInfo = new CultureInfo(newCultureName);
                    cultureNameProperty.stringValue = newCultureInfo.Name;
                }
                catch (Exception)
                {
                    cultureNameProperty.stringValue = CultureInfo.InvariantCulture.Name;
                }
            }

            CultureInfo target = new CultureInfo(cultureNameProperty.stringValue);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "English Name", target.EnglishName);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Native Name", target.NativeName);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Three Letter ISO Name", target.ThreeLetterISOLanguageName);

            EditorGUI.EndProperty();
        }
    }
}