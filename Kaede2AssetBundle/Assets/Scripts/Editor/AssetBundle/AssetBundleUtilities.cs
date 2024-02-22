using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Assets.Editor.AssetBundle
{
    public static class AssetBundleUtilities
    {
        private static readonly string ProjectDir = Path.GetDirectoryName(Application.dataPath)!;
        private static readonly string BaseDir = Path.Combine(Application.dataPath, "AssetBundles");
        private static readonly string TargetDir = Path.GetDirectoryName(ProjectDir)!;

        [MenuItem("Kaede2/Asset Bundles/Tag")]
        public static void TagBundles()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

            DirectoryInfo[] assetBaseDirs = new DirectoryInfo(BaseDir).GetDirectories();

            List<DirectoryInfo> assetBundleDirs = new List<DirectoryInfo>();

            // MasterData
            if (assetBaseDirs.All(d => d.Name != "MasterData"))
            {
                Debug.LogError("MasterData folder not found");
                return;
            }

            assetBundleDirs.Add(assetBaseDirs.First(d => d.Name == "MasterData"));

            // Scenarios
            if (assetBaseDirs.All(d => d.Name != "Scenarios"))
            {
                Debug.LogError("Scenarios folder not found");
                return;
            }

            DirectoryInfo scenariosDir = assetBaseDirs.First(d => d.Name == "Scenarios");
            assetBundleDirs.AddRange(scenariosDir.GetDirectories());

            // Set Labels
            foreach (var assetBundleDir in assetBundleDirs)
            {
                List<FileInfo> files = CollectAssets(assetBundleDir);

                string assetBundleFolderRelativePath = Path.GetRelativePath(BaseDir, assetBundleDir.FullName);
                string assetBundleName = assetBundleFolderRelativePath.Replace('\\', '/').ToLower(CultureInfo.InvariantCulture);

                foreach (var fileInfo in files)
                {
                    SetFileAssetBundleLabel(fileInfo, assetBundleName);
                }
            }
        }

        [MenuItem("Kaede2/Asset Bundles/Build/Current Target")]
        public static void BuildAssetBundlesCurrentTarget()
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        private static void BuildAssetBundles(BuildTarget buildTarget)
        {
            var targetDir = Path.Combine(TargetDir, "AssetBundles", $"{buildTarget:G}");
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            Directory.CreateDirectory(targetDir);
            BuildPipeline.BuildAssetBundles(targetDir, BuildAssetBundleOptions.DisableWriteTypeTree, buildTarget);
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

        private static void SetFileAssetBundleLabel(FileInfo fileInfoObj, string name)
        {
            if (fileInfoObj.Extension == ".meta") return;

            string assetPath = Path.GetRelativePath(ProjectDir, fileInfoObj.FullName);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            importer.SetAssetBundleNameAndVariant(name, "");
        }
    }
}