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

            // temporarily disable the tagger to avoid unnecessary tagging
            tagger.Enabled = false;
            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                // reimport progress info doesn't contain "Assets/" prefix so we do the same to make it look better
                path = path["Assets/".Length..];

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
                webSettings.format = TextureImporterFormat.ASTC_6x6;
                webSettings.textureCompression = TextureImporterCompression.Compressed;
                webSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                webSettings.crunchedCompression = true;
                webSettings.compressionQuality = 100;
                importer.SetPlatformTextureSettings(webSettings);

                importer.SaveAndReimport();
            }
            tagger.Enabled = true;
            // apply quality should not change the addressables, so we don't need to tag again
            // tagger.Apply();

            EditorUtility.ClearProgressBar();
        }
    }
}
