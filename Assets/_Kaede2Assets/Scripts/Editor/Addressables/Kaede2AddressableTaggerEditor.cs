using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Addressables
{
    [CustomEditor(typeof(Kaede2AddressableTagger))]
    public class Kaede2AddressableTaggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Kaede2AddressableTagger targetObject = (Kaede2AddressableTagger) target;
            base.OnInspectorGUI();

            if (GUILayout.Button("Apply"))
            {
                targetObject.Apply();
            }
        }
    }
}