using Kaede2.UI;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(LabeledListLayout))]
    public class LabeledListLayoutEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                switch (iterator.propertyPath)
                {
                    case "m_Script":
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.PropertyField(iterator, false);
                        EditorGUI.EndDisabledGroup();
                        continue;
                    // TODO: automatically check if the property should be displayed instead of hardcoding it
                    case "childLayoutGroup":
                        EditorGUILayout.PropertyField(iterator, true);
                        break;
                    default:
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}