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
            var localeCount = LocalizationManager.LoadAsset().All.Count;
            return EditorGUIUtility.singleLineHeight +
                   localeCount * GetSinglePropertyHeight() +
                   (localeCount - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valuesProperty = property.FindPropertyRelative("localizedValues");

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(position, label);
            EditorGUI.EndProperty();

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel += 1;
            foreach (var cultureInfo in LocalizationManager.LoadAsset().All)
            {
                EditorGUI.BeginChangeCheck();
                var oldValue = TryGetFromSerializedDictionary(valuesProperty, cultureInfo, out var value, out var serializedValue)
                    ? value
                    : default;
                string entryLabel = $"{cultureInfo.NativeName} / {cultureInfo.EnglishName}";

                if (serializedValue != null) EditorGUI.BeginProperty(position, new GUIContent(entryLabel), serializedValue);
                var newValue = DrawValueField(position, new GUIContent(entryLabel), oldValue);
                if (serializedValue != null)  EditorGUI.EndProperty();

                if (EditorGUI.EndChangeCheck())
                {
                    SetToSerializedDictionary(valuesProperty, cultureInfo, newValue);
                }

                position.y += GetSinglePropertyHeight() + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel -= 1;

        }

        protected abstract T GetValueFromSerializedProperty(SerializedProperty property);
        protected abstract void SetValueToSerializedProperty(SerializedProperty property, T value);
        protected abstract T DrawValueField(Rect position, GUIContent label, T value);
        protected abstract float GetSinglePropertyHeight();

        private bool TryGetFromSerializedDictionary(SerializedProperty dictionaryProperty, CultureInfo key, out T value, out SerializedProperty serializedValue)
        {
            var dictionaryPairsProperty = dictionaryProperty.FindPropertyRelative("pairs");

            for (int i = 0; i < dictionaryPairsProperty.arraySize; i++)
            {
                var pairElement = dictionaryPairsProperty.GetArrayElementAtIndex(i);
                var keyElement = pairElement.FindPropertyRelative("key");
                var keyCultureName = keyElement.FindPropertyRelative("cultureName").stringValue;
                if (keyCultureName == key.Name)
                {
                    serializedValue = pairElement.FindPropertyRelative("value");
                    value = GetValueFromSerializedProperty(serializedValue);
                    return true;
                }
            }

            serializedValue = null;
            value = default;
            return false;
        }

        private void SetToSerializedDictionary(SerializedProperty dictionaryProperty, CultureInfo key, T value)
        {
            var dictionaryPairsProperty = dictionaryProperty.FindPropertyRelative("pairs");

            for (int i = 0; i < dictionaryPairsProperty.arraySize; i++)
            {
                var pairElement = dictionaryPairsProperty.GetArrayElementAtIndex(i);
                var keyElement = pairElement.FindPropertyRelative("key");
                var keyCultureName = keyElement.FindPropertyRelative("cultureName").stringValue;
                if (keyCultureName == key.Name)
                {
                    var valueElement = pairElement.FindPropertyRelative("value");
                    SetValueToSerializedProperty(valueElement, value);
                    return;
                }
            }

            dictionaryPairsProperty.arraySize++;

            var newPairElement = dictionaryPairsProperty.GetArrayElementAtIndex(dictionaryPairsProperty.arraySize - 1);
            var newKeyElement = newPairElement.FindPropertyRelative("key");
            newKeyElement.FindPropertyRelative("cultureName").stringValue = key.Name;

            var newValueElement = newPairElement.FindPropertyRelative("value");
            SetValueToSerializedProperty(newValueElement, value);
        }
    }

    [CustomPropertyDrawer(typeof(LocalizedValue<float>))]
    public class LocalizedFloatDrawer : LocalizedItemDrawer<float>
    {
        protected override float GetValueFromSerializedProperty(SerializedProperty property)
        {
            return property.floatValue;
        }

        protected override void SetValueToSerializedProperty(SerializedProperty property, float value)
        {
            property.floatValue = value;
        }

        protected override float DrawValueField(Rect position, GUIContent label, float value)
        {
            return EditorGUI.FloatField(position, label, value);
        }

        protected override float GetSinglePropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
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
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, label);

            position.height = EditorGUIUtility.singleLineHeight * 3;
            position.y += EditorGUIUtility.singleLineHeight;
            return EditorGUI.TextArea(position, value);
        }

        protected override float GetSinglePropertyHeight()
        {
            return 4 * EditorGUIUtility.singleLineHeight;
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

        protected override float GetSinglePropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}