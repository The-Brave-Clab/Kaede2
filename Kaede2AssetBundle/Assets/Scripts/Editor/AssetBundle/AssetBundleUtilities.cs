using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Kaede2.Assets.AssetBundles;

namespace Kaede2.Assets.Editor.AssetBundle
{
    public static partial class AssetBundleUtilities
    {
        private static readonly string ProjectDir = Path.GetDirectoryName(Application.dataPath)!;

        [MenuItem("Kaede2/Asset Bundles/Tag")]
        public static void TagBundles()
        {
            List<FileInfo> files = CollectAssets(new DirectoryInfo(AssetBundleManifestData.ResourceBasePath));

            foreach (var fileInfo in files)
            {
                SetFileAssetBundleLabel(fileInfo);
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        [MenuItem("Kaede2/Asset Bundles/Build/Current Target")]
        public static void BuildAssetBundlesCurrentTarget()
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        private static void BuildAssetBundles(BuildTarget buildTarget)
        {
            var targetDir = AssetBundleManifestData.BasePath;
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }

            Directory.CreateDirectory(targetDir);
            var manifest = BuildPipeline.BuildAssetBundles(targetDir, BuildAssetBundleOptions.AssetBundleStripUnityVersion | BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
            GenerateJsonManifest(manifest, buildTarget);
        }

        private static List<FileInfo> CollectAssets(DirectoryInfo directoryInfo)
        {
            List<FileInfo> result = new List<FileInfo>();
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();

            foreach (var fileInfo in fileSystemInfos)
            {
                switch (fileInfo)
                {
                    case FileInfo fileInfoObj:
                        result.Add(fileInfoObj);
                        break;
                    case DirectoryInfo directoryInfoObj:
                        result = result.Concat(CollectAssets(directoryInfoObj)).ToList();
                        break;
                }
            }

            return result;
        }

        private static void SetFileAssetBundleLabel(FileInfo fileInfoObj)
        {
            if (fileInfoObj.Extension == ".meta") return;

            string assetPath = Path.GetRelativePath(ProjectDir, fileInfoObj.FullName);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            string assetBundleFolderRelativePath = Path.GetRelativePath(AssetBundleManifestData.ResourceBasePath, fileInfoObj.Directory!.FullName);
            string assetBundleName = assetBundleFolderRelativePath.Replace('\\', '/').ToLower(CultureInfo.InvariantCulture);

            // for scenario, only keep the first two path segments
            if (assetBundleName.StartsWith("scenario/"))
            {
                string[] segments = assetBundleName.Split('/');
                assetBundleName = string.Join("/", segments.Take(2));
            }

            // for live2d, only keep the first three path segments
            if (assetBundleName.StartsWith("scenario_common/live2d/"))
            {
                string[] segments = assetBundleName.Split('/');
                assetBundleName = string.Join("/", segments.Take(3));
            }

            if (importer.assetBundleName != assetBundleName)
                importer.SetAssetBundleNameAndVariant(assetBundleName, "");
        }
    }
}