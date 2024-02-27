using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Kaede2.Editor.Addressables
{
    [CreateAssetMenu(fileName = nameof(Kaede2AddressableTagger), menuName = "Kaede2/Editor/Addressable Tagger")]
    public class Kaede2AddressableTagger : ScriptableObject
    {
        [SerializeField] private string addressableGroupName = "Kaede2";
        [SerializeField] private Object addressableBaseFolder;

        public string AddressableBaseFolder => addressableBaseFolder == null ? null : AssetDatabase.GetAssetPath(addressableBaseFolder);

        private const string ProgressBarTitle = "Tagging Kaede2 Addressable Assets";

        public void Apply()
        {
            if (addressableBaseFolder == null) Debug.LogError("AddressableBaseFolder is not set.");
            if (addressableBaseFolder is not DefaultAsset) Debug.LogError("AddressableBaseFolder is not a folder.");
            string baseFolder = AddressableBaseFolder;
            if (!Directory.Exists(baseFolder)) Debug.LogError("AddressableBaseFolder is not a folder.");

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Preparing...", 0);
            AddressableAssetGroup assetGroup = settings.FindGroup(addressableGroupName);
            if (assetGroup != null)
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, "Removing Existing Asset Groups...", 0);
                settings.RemoveGroup(assetGroup);
            }
            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Creating New Asset Groups...", 0);
            assetGroup = settings.CreateGroup(addressableGroupName, false, false, false, settings.DefaultGroup.Schemas);

            List<string> currentLabels = settings.GetLabels().Where(l => l.StartsWith("kaede2")).ToList();
            List<string> unusedLabels = currentLabels.ToList();

            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Tagging Assets...", 0);

            int processedCount = 0;
            string[] folders = Directory.GetDirectories(baseFolder, "*", SearchOption.AllDirectories);
            foreach (var folder in folders)
            {
                var progressStr = $"{processedCount}/{folders.Length}";
                var progress = (float)processedCount / folders.Length;
                ++processedCount;
                EditorUtility.DisplayProgressBar(ProgressBarTitle, $"Tagging Assets ({progressStr})...", progress);

                if (!Filter(baseFolder, folder, out var bundleName)) continue;

                EditorUtility.DisplayProgressBar(ProgressBarTitle, $"Tagging {bundleName} ({progressStr})...", progress);

                var label = $"kaede2/{bundleName}";
                var address = bundleName;

                if (!currentLabels.Contains(label))
                    settings.AddLabel(label);
                else if (unusedLabels.Contains(label))
                    unusedLabels.Remove(label);

                var guid = AssetDatabase.AssetPathToGUID(folder);

                var entry = settings.CreateOrMoveEntry(guid, assetGroup);
                entry.labels.Add(label);
                entry.address = address;

                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }

            foreach (var label in unusedLabels)
            {
                settings.RemoveLabel(label);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

        public bool Filter(string baseFolder, string assetPath, out string bundleName)
        {
            bundleName = "";
            if (Path.HasExtension(assetPath))
                return false;

            bundleName = Path.GetRelativePath(baseFolder, assetPath);
            bundleName = bundleName.Replace('\\', '/');
            bundleName = bundleName.Trim('/');
            bundleName = bundleName.ToLower(CultureInfo.InvariantCulture);

            if (bundleName.StartsWith(".."))
                return false;

            var segments = bundleName.Split('/');

            if (segments[0] == "audio")
                return segments.Length == 2;

            if (segments[0] == "scenario")
                return segments.Length == 2;

            if (segments[0] == "scenario_common")
            {
                if (segments.Length == 3 && segments[1] == "live2d")
                    return true;

                if (segments.Length == 2)
                    return true;

                return false;
            }

            return segments.Length == 1;
        }

        [MenuItem("Kaede2/Addressables/Tag")]
        public static void Tag()
        {
            var taggerGUIDs = AssetDatabase.FindAssets($"t:{nameof(Kaede2AddressableTagger)}");
            if (taggerGUIDs.Length == 0)
            {
                Debug.LogError("Kaede2AddressableTagger not found.");
                return;
            }

            var taggerPath = AssetDatabase.GUIDToAssetPath(taggerGUIDs[0]);
            var tagger = AssetDatabase.LoadAssetAtPath<Kaede2AddressableTagger>(taggerPath);
            if (tagger == null)
            {
                Debug.LogError("Kaede2AddressableTagger not found.");
                return;
            }

            tagger.Apply();
        }
    }
}