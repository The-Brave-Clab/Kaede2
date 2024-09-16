using System;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Windows
{
    public class EditSave : EditorWindow
    {
        private SerializedSaveData serializedSaveData;
        private SerializedProperty saveDataProperty;
        
        private SerializedSettings serializedSettings;
        private SerializedProperty settingsProperty;

        [MenuItem("Kaede2/Windows/Edit Save")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditSave>("Edit Save");
            window.titleContent = new GUIContent("Edit Save");
        }

        private void OnGUI()
        {
            if (serializedSaveData == null || saveDataProperty == null)
            {
                SaveData.Load();
                if (serializedSaveData != null) DestroyImmediate(serializedSaveData);
                serializedSaveData = CreateInstance<SerializedSaveData>();
                saveDataProperty = new SerializedObject(serializedSaveData).FindProperty(nameof(SerializedSaveData.Data));
            }
            
            if (serializedSettings == null || settingsProperty == null)
            {
                GameSettings.Load();
                if (serializedSettings != null) DestroyImmediate(serializedSettings);
                serializedSettings = CreateInstance<SerializedSettings>();
                settingsProperty = new SerializedObject(serializedSettings).FindProperty(nameof(SerializedSettings.Settings));
            }

            EditorGUILayout.PropertyField(saveDataProperty, true);
            if (GUILayout.Button("Save"))
            {
                saveDataProperty.serializedObject.ApplyModifiedProperties();
                SaveData.Save();
            }
            
            EditorGUILayout.PropertyField(settingsProperty, true);
            if (GUILayout.Button("Save"))
            {
                settingsProperty.serializedObject.ApplyModifiedProperties();
                GameSettings.Save();
            }
        }

        private class SerializedSaveData : ScriptableObject
        {
            public SaveData Data = null;

            private void OnEnable()
            {
                Data = SaveData.Instance;
            }
        }

        private class SerializedSettings : ScriptableObject
        {
            public GameSettings Settings = null;

            private void OnEnable()
            {
                Settings = GameSettings.Instance;
            }
        }
    }
}