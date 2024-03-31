using Kaede2.UI.Framework;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(SelectableGroup))]
    public class SelectableGroupEditor : UnityEditor.Editor
    {
        private SelectableGroup component;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            component = (SelectableGroup)target;
            if (!Application.isPlaying) return;

            if (GUILayout.Button(nameof(component.Previous)))
                component.Previous();

            if (GUILayout.Button(nameof(component.Next)))
                component.Next();

            if (GUILayout.Button(nameof(component.Confirm)))
                component.Confirm();
        }
    }
}