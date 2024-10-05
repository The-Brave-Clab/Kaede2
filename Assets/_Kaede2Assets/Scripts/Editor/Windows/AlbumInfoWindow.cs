using System;
using Kaede2.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Windows
{
    public class AlbumInfoWindow : EditorWindow
    {
        [MenuItem("Kaede2/Windows/Album Info")]
        public static void ShowWindow()
        {
            var inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var window = GetWindow<AlbumInfoWindow>("Album Info", inspectorWindowType);
            window.titleContent = new GUIContent("Album Info");
        }

        private string albumName;
        private Texture2D albumIllust;
        private MasterAlbumInfo.AlbumInfo albumInfo;
        private AlbumExtraInfo extraInfoObject;
        private SerializedObject infoSerializedObject;
        private SerializedObject extraInfoSerializedObject;

        private void Reload()
        {
            albumIllust = null;
            albumInfo = null;
            var objectAssets = AssetDatabase.FindAssets($"t:{nameof(AlbumExtraInfo)}");
            if (objectAssets.Length > 0)
            {
                extraInfoObject = AssetDatabase.LoadAssetAtPath<AlbumExtraInfo>(AssetDatabase.GUIDToAssetPath(objectAssets[0]));
                extraInfoSerializedObject = new SerializedObject(extraInfoObject);
            }
            infoSerializedObject = new SerializedObject(MasterAlbumInfo.Instance);
        }

        private void OnEnable()
        {
            albumName = "";
            Reload();
        }

        private void OnGUI()
        {
            if (extraInfoObject == null)
            {
                EditorGUILayout.HelpBox("No AlbumExtraInfo object found", MessageType.Error);
                return;
            }

            if (GUILayout.Button("Reload"))
            {
                Reload();
            }

            albumName = EditorGUILayout.TextField("Album Name", albumName);
            int index = -1;
            for (var i = 0; i < MasterAlbumInfo.Sorted.Count; i++)
            {
                if (MasterAlbumInfo.Sorted[i].AlbumName.EndsWith(albumName) && !MasterAlbumInfo.Sorted[i].AlbumName.StartsWith("card_character"))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                EditorGUILayout.HelpBox($"{albumName} not found", MessageType.Info);
                return;
            }

            var prevNextButtonRect = EditorGUILayout.BeginHorizontal();

            int Get(int index2, bool directionIsNext)
            {
                int newIndex = index2;
                if (directionIsNext)
                {
                    newIndex++;
                    while (newIndex < MasterAlbumInfo.Sorted.Count && MasterAlbumInfo.Sorted[newIndex].AlbumName.StartsWith("card_character"))
                    {
                        newIndex++;
                    }
                }
                else
                {
                    newIndex--;
                    while (newIndex >= 0 && MasterAlbumInfo.Sorted[newIndex].AlbumName.StartsWith("card_character"))
                    {
                        newIndex--;
                    }
                }

                return newIndex;
            }

            var prevIndex = Get(index, false);
            EditorGUI.BeginDisabledGroup(prevIndex < 0);
            if (GUILayout.Button("Prev"))
            {
                index = prevIndex;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();

            var nextIndex = Get(index, true);
            EditorGUI.BeginDisabledGroup(nextIndex >= MasterAlbumInfo.Sorted.Count);
            if (GUILayout.Button("Next"))
            {
                index = nextIndex;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            albumName = MasterAlbumInfo.Sorted[index].AlbumName.Split('_')[^1];
            if (albumInfo == null || !albumInfo.AlbumName.EndsWith(albumName))
            {
                albumInfo = MasterAlbumInfo.Sorted[index];
                // albumInfo refreshed, trigger reload
                albumIllust = null;
            }

            if (albumIllust == null)
            {
                albumIllust = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/AddressableAssets/illust/original/card_image_{albumName}.png");
                if (albumIllust == null)
                {
                    EditorGUILayout.HelpBox($"Illustration {albumName} not found", MessageType.Error);
                    return;
                }
            }

            var albumIllustRect = EditorGUILayout.GetControlRect(GUILayout.Height(200));
            EditorGUI.DrawPreviewTexture(albumIllustRect, albumIllust, null, ScaleMode.ScaleToFit);

            void DrawAllNoFoldout(SerializedProperty iterator)
            {
                int startDepth = iterator.depth;
                int lastDepth = startDepth;
                while (iterator.NextVisible(true))
                {
                    if (iterator.depth < startDepth) break;
                    if (iterator.depth < lastDepth)
                    {
                        EditorGUI.indentLevel--;
                    }
                    else if (iterator.depth > lastDepth)
                    {
                        EditorGUI.indentLevel++;
                    }
                    lastDepth = iterator.depth;
                    EditorGUILayout.PropertyField(iterator, false);
                }
            }

            // draw extra info
            int extraInfoIndex = -1;
            for (var i = 0; i < extraInfoObject.list.Length; i++)
            {
                if (extraInfoObject.list[i].name.EndsWith(albumName) && !extraInfoObject.list[i].name.StartsWith("card_character"))
                {
                    extraInfoIndex = i;
                    break;
                }
            }

            if (extraInfoIndex < 0)
            {
                EditorGUILayout.HelpBox("Extra info not found", MessageType.Error);
                return;
            }
            
            // var extraInfo = extraInfoObject.list[extraInfoIndex];
            var extraInfoListSerialized = extraInfoSerializedObject.FindProperty(nameof(extraInfoObject.list));
            var extraInfoSerialized = extraInfoListSerialized.GetArrayElementAtIndex(extraInfoIndex);
            var iterator = extraInfoSerialized.Copy();
            iterator.NextVisible(true);

            EditorGUILayout.LabelField("Extra Info", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            DrawAllNoFoldout(iterator);

            EditorGUI.indentLevel--;
    
            if (EditorGUI.EndChangeCheck())
            {
                extraInfoSerializedObject.ApplyModifiedProperties();
                extraInfoSerializedObject.Update();
                EditorUtility.SetDirty(extraInfoObject);
                AssetDatabase.SaveAssets();
            }

            // draw info
            EditorGUILayout.LabelField("Album Info", EditorStyles.boldLabel);

            var originalIndex = -1;
            for (var i = 0; i < MasterAlbumInfo.Instance.Data.Length; i++)
            {
                if (MasterAlbumInfo.Instance.Data[i].AlbumName == MasterAlbumInfo.Sorted[index].AlbumName)
                {
                    originalIndex = i;
                    break;
                }
            }

            if (originalIndex < 0)
            {
                EditorGUILayout.HelpBox("Original info not found, which should not happen", MessageType.Error);
                return;
            }

            var infoListSerialized = infoSerializedObject.FindProperty("albumInfo");
            var infoSerialized = infoListSerialized.GetArrayElementAtIndex(originalIndex);
            iterator = infoSerialized.Copy();
            iterator.NextVisible(true);

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(true);

            DrawAllNoFoldout(iterator);

            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }
    }
}