using System.Globalization;
using Kaede2.Localization;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    public abstract class LocalizedItemDrawer<T> : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (Locales.Load().All.Count + 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var valuesProperty = property.FindPropertyRelative("localizedValues");

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, label);

            EditorGUI.indentLevel += 1;
            foreach (var cultureInfo in Locales.Load().All)
            {
                position.y += EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginChangeCheck();
                var oldValue = TryGetFromSerializedDictionary(valuesProperty, cultureInfo, out var value)
                    ? value
                    : default;
                string entryLabel = $"{cultureInfo.NativeName} / {cultureInfo.EnglishName}";
                var newValue = DrawValueField(position, new GUIContent(entryLabel), oldValue);
                if (EditorGUI.EndChangeCheck())
                {
                    SetToSerializedDictionary(valuesProperty, cultureInfo, newValue);
                }
            }
            EditorGUI.indentLevel -= 1;

            EditorGUI.EndProperty();
        }

        protected abstract T GetValueFromSerializedProperty(SerializedProperty property);
        protected abstract void SetValueToSerializedProperty(SerializedProperty property, T value);
        protected abstract T DrawValueField(Rect position, GUIContent label, T value);

        private bool TryGetFromSerializedDictionary(SerializedProperty dictionaryProperty, CultureInfo key, out T value)
        {
            var dictionaryKeysProperty = dictionaryProperty.FindPropertyRelative("keys");
            var dictionaryValuesProperty = dictionaryProperty.FindPropertyRelative("values");

            for (int i = 0; i < dictionaryKeysProperty.arraySize; i++)
            {
                var element = dictionaryKeysProperty.GetArrayElementAtIndex(i);
                var keyCultureName = element.FindPropertyRelative("cultureName").stringValue;
                if (keyCultureName == key.Name)
                {
                    value = GetValueFromSerializedProperty(dictionaryValuesProperty.GetArrayElementAtIndex(i));
                    return true;
                }
            }

            value = default;
            return false;
        }

        private void SetToSerializedDictionary(SerializedProperty dictionaryProperty, CultureInfo key, T value)
        {
            var dictionaryKeysProperty = dictionaryProperty.FindPropertyRelative("keys");
            var dictionaryValuesProperty = dictionaryProperty.FindPropertyRelative("values");

            for (int i = 0; i < dictionaryKeysProperty.arraySize; i++)
            {
                var element = dictionaryKeysProperty.GetArrayElementAtIndex(i);
                var keyCultureName = element.FindPropertyRelative("cultureName").stringValue;
                if (keyCultureName == key.Name)
                {
                    var valueElement = dictionaryValuesProperty.GetArrayElementAtIndex(i);
                    SetValueToSerializedProperty(valueElement, value);
                    return;
                }
            }

            dictionaryKeysProperty.arraySize++;
            dictionaryValuesProperty.arraySize++;

            var newKeyElement = dictionaryKeysProperty.GetArrayElementAtIndex(dictionaryKeysProperty.arraySize - 1);
            newKeyElement.FindPropertyRelative("cultureName").stringValue = key.Name;

            var newValueElement = dictionaryValuesProperty.GetArrayElementAtIndex(dictionaryValuesProperty.arraySize - 1);
            SetValueToSerializedProperty(newValueElement, value);
        }
    }

    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : LocalizedItemDrawer<string>
    {
        protected override string GetValueFromSerializedProperty(SerializedProperty property)
        {
            return property.stringValue;
        }

        protected override void SetValueToSerializedProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
        }

        protected override string DrawValueField(Rect position, GUIContent label, string value)
        {
            return EditorGUI.TextField(position, label, value);
        }
    }

    [CustomPropertyDrawer(typeof(LocalizedAsset<>))]
    public class LocalizedAssetDrawer : LocalizedItemDrawer<Object>
    {
        protected override Object GetValueFromSerializedProperty(SerializedProperty property)
        {
            return property.objectReferenceValue;
        }

        protected override void SetValueToSerializedProperty(SerializedProperty property, Object value)
        {
            property.objectReferenceValue = value;
        }

        protected override Object DrawValueField(Rect position, GUIContent label, Object value)
        {
            // get generic type from fieldInfo
            var genericType = fieldInfo.FieldType.GetGenericArguments()[0];

            return EditorGUI.ObjectField(position, label, value, genericType, false);
        }
    }
}