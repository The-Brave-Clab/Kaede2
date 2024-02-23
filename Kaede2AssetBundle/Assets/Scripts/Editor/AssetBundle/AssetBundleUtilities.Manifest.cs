using System.IO;
using UnityEditor;
using Kaede2.Assets.AssetBundles;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Kaede2.Assets.Editor.AssetBundle
{
    public static partial class AssetBundleUtilities
    {
        private class UnityManifest
        {
            public class HashData
            {
                public int serializedVersion;
                public string Hash;
            }

            public class AssetBundleHashData
            {
                public HashData AssetFileHash;
                public HashData TypeTreeHash;
                public HashData IncrementalBuildHash;
            }

            public int ManifestFileVersion;
            public string UnityVersion;
            public uint CRC;
            public AssetBundleHashData Hashes;
            // ignore all other fields
        }

        private static void GenerateJsonManifest(AssetBundleManifest manifest, BuildTarget buildTarget)
        {
            var allAssetBundles = manifest.GetAllAssetBundles();
            AssetBundleManifestData manifestData = new() { manifests = new() };

            foreach (var assetBundle in allAssetBundles)
            {
                manifestData.manifests.Add(ReadManifest(assetBundle, buildTarget));
            }

            var json = JsonUtility.ToJson(manifestData, true);
            var jsonPath = Path.Combine(AssetBundleManifestData.BasePath, AssetBundleManifestData.ManifestFileName);
            File.WriteAllText(jsonPath, json);
        }

        private static AssetBundleManifestData.Manifest ReadManifest(string bundleName, BuildTarget buildTarget)
        {
            // Read Manifest File
            var manifestPath = Path.Combine(AssetBundleManifestData.BasePath, $"{bundleName}.manifest");
            var manifestText = File.ReadAllText(manifestPath);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            var unityManifest = deserializer.Deserialize<UnityManifest>(manifestText);
            File.Delete(manifestPath);

            return new()
            {
                name = bundleName,
                hash = unityManifest.Hashes.AssetFileHash.Hash,
                crc = unityManifest.CRC
            };
        }
    }
}