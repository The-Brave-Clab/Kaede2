using System.Linq;
using UnityEditor;

namespace Kaede2.Editor.Addressables
{
    public class Kaede2AddressableAutoApplier : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var taggerGUIDs = AssetDatabase.FindAssets($"t:{nameof(Kaede2AddressableTagger)}");
            if (taggerGUIDs.Length == 0)
                return;

            var taggerPath = AssetDatabase.GUIDToAssetPath(taggerGUIDs[0]);
            var tagger = AssetDatabase.LoadAssetAtPath<Kaede2AddressableTagger>(taggerPath);
            if (tagger == null)
                return;

            if (NeedApply(tagger, importedAssets) || NeedApply(tagger, deletedAssets) || NeedApply(tagger, movedAssets) || NeedApply(tagger, movedFromAssetPaths))
            {
                tagger.Apply();
            }
        }

        static bool NeedApply(Kaede2AddressableTagger tagger, string[] assets)
        {
            var baseFolder = tagger.AddressableBaseFolder;
            return assets.Any(a => tagger.Filter(baseFolder, a, out _));
        }
    }
}