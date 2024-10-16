﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kaede2.Scenario;
using Kaede2.Scenario.Framework.Editor.Inspectors;
using Kaede2.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(PlayerScenarioModule))]
    public class PlayerScenarioModuleEditor : ScenarioModuleEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                switch (iterator.propertyPath)
                {
                    case nameof(PlayerScenarioModule.defaultScenarioName):
                        DrawScenarioSelector(iterator);
                        break;
                    case "m_Script":
                        continue;
                    default:
                        EditorGUILayout.PropertyField(iterator, true);
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private MasterScenarioInfo masterData;
        private List<MasterScenarioInfo.ScenarioInfo> sortedScenarioInfo;

        private int GetIndexOfScenarioName(string scenarioName)
        {
            return sortedScenarioInfo.FindIndex(si => si.ScenarioName == scenarioName);
        }

        private void DrawScenarioSelector(SerializedProperty property)
        {
            // draw header first
            FieldInfo fieldInfo = typeof(PlayerScenarioModule).GetField(property.name, BindingFlags.Public | BindingFlags.Instance);

            HeaderAttribute headerAttr = fieldInfo!.GetCustomAttribute<HeaderAttribute>();
            if (headerAttr != null)
            {
                EditorGUILayout.LabelField(headerAttr.header, EditorStyles.boldLabel);
            }

            // original scenario string field
            string originalValue = property.stringValue;
            string newValue = EditorGUILayout.TextField(property.displayName, originalValue);
            if (originalValue != newValue)
            {
                if (GetIndexOfScenarioName(newValue) >= 0)
                {
                    property.stringValue = newValue;
                }
            }

            // get master data for popup
            const string noMasterDataMessage = "Quick selection is not available due to MasterScenarioInfo.masterdata not found.";
            if (masterData == null)
            {
                masterData = MasterScenarioInfo.Instance;
                if (masterData == null)
                {
                    EditorGUILayout.HelpBox(noMasterDataMessage, MessageType.Warning);
                    return;
                }
            }

            // draw popup
            sortedScenarioInfo ??= masterData.Data
                .OrderBy(si => si.Id)
                .ToList();

            string[] popupOptions = sortedScenarioInfo
                .Select(si => $"{si.KindName}/{si.ChapterName}/【{si.EpisodeNumber}】{si.EpisodeName}/【{si.Label}】{si.StoryName}")
                .ToArray();

            int originalIndex = GetIndexOfScenarioName(property.stringValue);
            int selection = EditorGUILayout.Popup(originalIndex, popupOptions.ToArray());

            if (selection != originalIndex)
            {
                property.stringValue = sortedScenarioInfo[selection].ScenarioName;
            }
        }
    }
}