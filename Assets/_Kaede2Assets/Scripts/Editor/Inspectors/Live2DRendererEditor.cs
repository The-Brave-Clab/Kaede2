using System.Linq;
using Kaede2.Scenario.Entities;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(Live2DActorEntity))]
    public class Live2DActorEntityEditor : UnityEditor.Editor
    {
        private bool showTestButtons = false;
        private bool showFaceButtons = false;
        private bool showMtnButtons = false;
        private Live2DActorEntity component = null;
        private void OnEnable()
        {
            //throw new NotImplementedException();
            component = target as Live2DActorEntity;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (component != null)
                DrawMotionTestButtons(component, ref showTestButtons, ref showFaceButtons, ref showMtnButtons);
        }

        private static void DrawMotionTestButtons(Live2DActorEntity controller, ref bool showButtons, ref bool showFace, ref bool showMtn)
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to Test Motions!", MessageType.Warning);
                return;
            }

            showButtons = EditorGUILayout.Foldout(showButtons, "Motion Test Buttons");
            if (showButtons)
            {
                EditorGUI.indentLevel += 1;

                var motionNames = controller.MotionNames.ToList();
                motionNames.Sort();

                showFace = EditorGUILayout.Foldout(showFace, "Face Motions");
                if (showFace)
                {
                    EditorGUI.indentLevel += 1;

                    foreach (var motionName in motionNames)
                    {
                        if (!motionName.StartsWith("face_")) continue;
                        if (GUILayout.Button(motionName))
                        {
                            controller.StartMotion(motionName);
                        }
                    }

                    EditorGUI.indentLevel -= 1;
                }

                showMtn = EditorGUILayout.Foldout(showMtn, "Body Motions");
                if (showMtn)
                {
                    EditorGUI.indentLevel += 1;

                    foreach (var motionName in motionNames)
                    {
                        if (!motionName.StartsWith("mtn_")) continue;
                        if (GUILayout.Button(motionName))
                        {
                            controller.StartMotion(motionName);
                        }
                    }
                    
                    EditorGUI.indentLevel -= 1;
                }

                EditorGUI.indentLevel -= 1;
            }
        }
    }

}