using System;
using Kaede2.Localization;
using UnityEditor;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(Locales))]
    public class LocalesEditor : UnityEditor.Editor
    {
        private Locales component;

        private void OnEnable()
        {
            component = (Locales)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}