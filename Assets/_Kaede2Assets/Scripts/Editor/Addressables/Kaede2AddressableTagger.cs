using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
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

        public bool Enabled { get; set; } = true;

        private void Awake()
        {
            Enabled = true;
        }

        public void Apply()
        {
            if (!Enabled) return;

            if (addressableBaseFolder == null) this.LogError("AddressableBaseFolder is not set.");
            if (addressableBaseFolder is not DefaultAsset) this.LogError("AddressableBaseFolder is not a folder.");
            string baseFolder = AddressableBaseFolder;
            if (!Directory.Exists(baseFolder)) this.LogError("AddressableBaseFolder is not a folder.");

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Preparing...", 0);
            AddressableAssetGroup assetGroup = settings.FindGroup(addressableGroupName);
            if (assetGroup == null)
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, "Creating New Asset Groups...", 0);
                assetGroup = settings.CreateGroup(addressableGroupName, false, false, false, settings.DefaultGroup.Schemas);
                // change build & load path to remote, keeping other settings default
                var bundledAssetGroupSchema = assetGroup.GetSchema<BundledAssetGroupSchema>();
                var idInfo = settings.profileSettings.GetProfileDataByName("Remote.BuildPath");
                bundledAssetGroupSchema.BuildPath.SetVariableById(settings, idInfo.Id);
                idInfo = settings.profileSettings.GetProfileDataByName("Remote.LoadPath");
                bundledAssetGroupSchema.LoadPath.SetVariableById(settings, idInfo.Id);
            }

            List<string> currentLabels = settings.GetLabels().Where(l => l.StartsWith("kaede2")).ToList();
            List<string> unusedLabels = currentLabels.ToList();

            List<AddressableAssetEntry> currentEntries = assetGroup.entries.ToList();
            List<AddressableAssetEntry> unusedEntries = currentEntries.ToList();

            EditorUtility.DisplayProgressBar(ProgressBarTitle, "Tagging Assets...", 0);

            int processedCount = 0;
            List<string> allAssets = new();
            allAssets.AddRange(Directory.GetDirectories(baseFolder, "*", SearchOption.AllDirectories));

            // special case for illust folder since the files under it will not be packed into one single bundle
            var illustFolder = Path.Combine(baseFolder, "illust");
            if (Directory.Exists(illustFolder))
            {
                // find all png files in illust folder
                allAssets.AddRange(Directory.GetFiles(illustFolder, "*.png", SearchOption.AllDirectories));
            }

            foreach (var asset in allAssets)
            {
                var progressStr = $"{processedCount}/{allAssets.Count}";
                var progress = (float)processedCount / allAssets.Count;
                ++processedCount;
                if (!Filter(baseFolder, asset, out var bundleName, out var address)) continue;

                if (processedCount % 10 == 0)
                    EditorUtility.DisplayProgressBar(ProgressBarTitle, $"Tagging {bundleName} ({progressStr})...", progress);

                var label = $"kaede2/{bundleName}";

                if (!currentLabels.Contains(label))
                    settings.AddLabel(label);
                else if (unusedLabels.Contains(label))
                    unusedLabels.Remove(label);

                var guid = AssetDatabase.AssetPathToGUID(asset);

                unusedEntries.RemoveAll(e => e.guid == guid);

                bool needModify = currentEntries.All(e => e.guid != guid) || // new entry
                                  currentEntries.Where(e => e.guid == guid) // existing entry that
                                      .Any(e => e.labels.Count != 1 || !e.labels.Contains(label) || e.address != address); // doesn't match

                if (needModify)
                {
                    var entry = settings.CreateOrMoveEntry(guid, assetGroup);
                    entry.labels.Clear();
                    entry.labels.Add(label);
                    entry.address = address;

                    settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                }
            }

            foreach (var label in unusedLabels)
            {
                settings.RemoveLabel(label);
            }

            foreach (var entry in unusedEntries)
            {
                settings.RemoveAssetEntry(entry.guid);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

        public bool Filter(string baseFolder, string assetPath, out string bundleName, out string address)
        {
            bundleName = "";
            address = "";
            if (string.IsNullOrEmpty(baseFolder) || string.IsNullOrEmpty(assetPath))
                return false;

            bundleName = Path.GetRelativePath(baseFolder, assetPath);
            bundleName = bundleName.Replace('\\', '/');
            bundleName = bundleName.Trim('/');
            address = bundleName;

            if (bundleName.StartsWith(".."))
                return false;

            var segments = bundleName.Split('/');

            // we only deal with illust files
            if (Path.HasExtension(segments[^1]) && segments[0] != "illust")
                return false;

            if (segments[0] == "audio")
                return segments.Length == 2;

            if (segments[0] == "scenario")
                return segments.Length == 2;

            if (segments[0] == "cartoon_images")
                return segments.Length == 2;

            if (segments[0] == "zukan")
                return segments.Length == 2;

            if (segments[0] == "character")
                return segments.Length == 2;

            // we don't include opening_movie and ui in addressables
            // it's in this folder but we use it as a common asset
            if (segments[0] == "opening_movie" || segments[0] == "ui")
                return false;

            if (segments[0] == "scenario_common")
            {
                if (segments.Length > 1)
                {
                    if (segments[1] == "live2d")
                        return segments.Length == 3;

                    // charaicon is used as common assets
                    if (segments[1] == "charaicon")
                        return false;
                }

                return segments.Length == 2;
            }

            if (segments[0] == "illust")
            {
                if (segments.Length == 3)
                {
                    if (segments[1] == "original" || segments[1] == "thumbnail")
                    {
                        string illustName = Path.GetFileNameWithoutExtension(segments[2]);
                        var bundleIndex = MasterAlbumInfo.GetBundleIndex(illustName);
                        // override bundle name, with address remains the same
                        bundleName = $"illust/{segments[1]}/{bundleIndex:00}";
                        return true;
                    }
                }

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
                typeof(Kaede2AddressableTagger).LogError("Kaede2AddressableTagger not found.");
                return;
            }

            var taggerPath = AssetDatabase.GUIDToAssetPath(taggerGUIDs[0]);
            var tagger = AssetDatabase.LoadAssetAtPath<Kaede2AddressableTagger>(taggerPath);
            if (tagger == null)
            {
                typeof(Kaede2AddressableTagger).LogError("Kaede2AddressableTagger not found.");
                return;
            }

            tagger.Apply();
        }
    }
}