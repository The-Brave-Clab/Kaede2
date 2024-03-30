using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Addressables
{
    public static class Kaede2AssetsQualityApplier
    {
        [MenuItem("Kaede2/Addressables/Apply Quality")]
        public static void Apply()
        {
            var taggerGUIDs = AssetDatabase.FindAssets($"t:{nameof(Kaede2AddressableTagger)}");
            if (taggerGUIDs.Length == 0)
                return;

            var taggerPath = AssetDatabase.GUIDToAssetPath(taggerGUIDs[0]);
            var tagger = AssetDatabase.LoadAssetAtPath<Kaede2AddressableTagger>(taggerPath);
            if (tagger == null)
                return;

            EditorUtility.DisplayProgressBar("Applying Quality", "", 0);

            var baseFolder = tagger.AddressableBaseFolder;

            var guids = AssetDatabase.FindAssets($"t:{nameof(Texture)}", new[] { baseFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                EditorUtility.DisplayProgressBar($"Applying Quality ({i + 1} / {guids.Length})", path, (float) i / guids.Length);

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                importer.filterMode = FilterMode.Trilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.crunchedCompression = false;

                var standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
                standaloneSettings.overridden = true;
                standaloneSettings.format = TextureImporterFormat.Automatic;
                standaloneSettings.textureCompression = TextureImporterCompression.CompressedHQ;
                standaloneSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                importer.SetPlatformTextureSettings(standaloneSettings);

                var androidSettings = importer.GetPlatformTextureSettings("Android");
                androidSettings.overridden = true;
                androidSettings.format = TextureImporterFormat.Automatic;
                androidSettings.textureCompression = TextureImporterCompression.Compressed;
                androidSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                importer.SetPlatformTextureSettings(androidSettings);

                var iosSettings = importer.GetPlatformTextureSettings("iPhone");
                iosSettings.overridden = true;
                iosSettings.format = TextureImporterFormat.Automatic;
                iosSettings.textureCompression = TextureImporterCompression.Compressed;
                iosSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                importer.SetPlatformTextureSettings(iosSettings);

                var webSettings = importer.GetPlatformTextureSettings("Web");
                webSettings.overridden = true;
                webSettings.format = TextureImporterFormat.Automatic;
                webSettings.textureCompression = TextureImporterCompression.Compressed;
                webSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                webSettings.crunchedCompression = true;
                webSettings.compressionQuality = 100;
                importer.SetPlatformTextureSettings(webSettings);
                
                importer.SaveAndReimport();
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
