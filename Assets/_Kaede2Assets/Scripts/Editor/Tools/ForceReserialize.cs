using System.Linq;
using TMPro;
using UnityEditor;

namespace Kaede2.Editor.Tools
{
    public static class ForceReserialize
    {
        [MenuItem("Kaede2/Tools/Force Reserialize All Assets")]
        public static void ForceReserializeAllAssets()
        {
            AssetDatabase.ForceReserializeAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Kaede2/Tools/Reset TextMeshPro Assets")]
        public static void ResetTMPAsset()
        {
            // load all TextMeshPro assets
            var allAssets = AssetDatabase.FindAssets($"t:{nameof(TMP_FontAsset)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>)
                .ToList();

            var clearDynamicDataOnBuild = typeof(TMP_FontAsset).GetProperty("clearDynamicDataOnBuild");
            // remove dynamic data
            foreach (var asset in allAssets)
            {
                if (asset.atlasPopulationMode == AtlasPopulationMode.Static) continue;

                asset.isMultiAtlasTexturesEnabled = true;
                // asset.clearDynamicDataOnBuild = true;
                // clearDynamicDataOnBuild is internal. Call with reflection.
                clearDynamicDataOnBuild?.SetValue(asset, true);
                asset.ClearFontAssetData(true);

                EditorUtility.SetDirty(asset);
            }
            TMP_ResourceManager.ClearFontAssetGlyphCache();
            AssetDatabase.SaveAssets();
        }
    }
}