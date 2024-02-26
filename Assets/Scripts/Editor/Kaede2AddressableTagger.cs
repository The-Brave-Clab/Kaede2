using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Kaede2.Editor
{
    [CreateAssetMenu(fileName = "Kaede2AddressableTagger", menuName = "Kaede2/Editor/Addressable Tagger")]
    public class Kaede2AddressableTagger : ScriptableObject
    {
        [SerializeField] private string addressableGroupName = "Kaede2";
        [SerializeField] private Object addressableBaseFolder;

        private const string ProgressBarTitle = "Tagging Kaede2 Addressable Assets...";

        public void Apply()
        {
            if (addressableBaseFolder == null) Debug.LogError("AddressableBaseFolder is not set.");
            if (addressableBaseFolder is not DefaultAsset) Debug.LogError("AddressableBaseFolder is not a folder.");
            string baseFolder = AssetDatabase.GetAssetPath(addressableBaseFolder);
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
            assetGroup = settings.CreateGroup(addressableGroupName, true, false, false, settings.DefaultGroup.Schemas);

            // clear labels
            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Removing Labels...", 0);
            var existingLabels = settings.GetLabels();
            for (var i = 0; i < existingLabels.Count; i++)
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, "Removing Labels...", (float)i / existingLabels.Count);
                if (existingLabels[i].StartsWith("/kaede2"))
                    settings.RemoveLabel(existingLabels[i]);
            }

            HashSet<string> labels = new();
            // get all folder assets in the base folder
            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Tagging Assets...", 0);
            string[] folders = Directory.GetDirectories(baseFolder, "*", SearchOption.AllDirectories);
            for (var i = 0; i < folders.Length; i++)
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, $"Tagging Assets ({i + 1}/{folders.Length})...", (float)i / folders.Length);

                var folder = folders[i];
                if (!Filter(baseFolder, folder, out var bundleName)) continue;

                EditorUtility.DisplayProgressBar(ProgressBarTitle, $"Tagging {bundleName} ({i + 1}/{folders.Length})...", (float)i / folders.Length);

                var label = $"/kaede2/{bundleName}";
                var address = bundleName;

                if (labels.Add(label))
                    settings.AddLabel(label);

                var guid = AssetDatabase.AssetPathToGUID(folder);

                var entry = settings.CreateOrMoveEntry(guid, assetGroup);
                entry.labels.Add(label);
                entry.address = address;

                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

        private bool Filter(string baseFolder, string assetPath, out string bundleName)
        {
            bundleName = Path.GetRelativePath(baseFolder, assetPath);
            bundleName = bundleName.Replace('\\', '/');
            bundleName = bundleName.Trim('/');
            bundleName = bundleName.ToLower(CultureInfo.InvariantCulture);

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
    }
}