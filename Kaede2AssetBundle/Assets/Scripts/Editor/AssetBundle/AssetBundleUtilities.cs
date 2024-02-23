using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Kaede2.Assets.Editor;

namespace Kaede2.Assets.Editor.AssetBundle
{
    public static partial class AssetBundleUtilities
    {
        private static readonly string ProjectDir = Path.GetDirectoryName(Application.dataPath)!;
        private static readonly string BaseDir = Path.Combine(Application.dataPath, "AssetBundles");
        private static readonly string TargetDir = Path.GetDirectoryName(ProjectDir)!;

        [MenuItem("Kaede2/Asset Bundles/Tag")]
        public static void TagBundles()
        {
            List<FileInfo> files = CollectAssets(new DirectoryInfo(BaseDir));

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
            var targetDir = GetTargetAssetBundlePath(buildTarget);
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }

            Directory.CreateDirectory(targetDir);
            var manifest = BuildPipeline.BuildAssetBundles(targetDir, BuildAssetBundleOptions.DisableWriteTypeTree, buildTarget);
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

            string assetBundleFolderRelativePath = Path.GetRelativePath(BaseDir, fileInfoObj.Directory!.FullName);
            string assetBundleName = assetBundleFolderRelativePath.Replace('\\', '/').ToLower(CultureInfo.InvariantCulture);

            if (assetBundleName.StartsWith("scenario"))
            {
                // for scenario, only keep the first two path segments
                string[] segments = assetBundleName.Split('/');
                assetBundleName = string.Join("/", segments.Take(2));
            }

            if (importer.assetBundleName != assetBundleName)
                importer.SetAssetBundleNameAndVariant(assetBundleName, "");
        }

        private static string GetTargetAssetBundlePath(BuildTarget buildTarget)
        {
            return Path.Combine(TargetDir, "AssetBundles", $"{EditorPlatformHelper.FromBuildTarget(buildTarget):G}");
        }
    }
}