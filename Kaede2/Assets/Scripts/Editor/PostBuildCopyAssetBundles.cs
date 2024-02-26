using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kaede2.Assets;
using Kaede2.Assets.AssetBundles;
using Kaede2.Assets.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Kaede2.Editor
{
    public class PostBuildCopyAssetBundles : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;

        public void OnPreprocessBuild(BuildReport report)
        {
            var assetBundlePath = AssetBundleManifestData.BasePath;
            var platform = EditorPlatformHelper.FromBuildTarget(report.summary.platform);

            if (!Directory.Exists(assetBundlePath))
            {
                Debug.LogError($"AssetBundle directory for platform {platform:G} not found! The build may not work properly.");
                return;
            }

            CopyAssetBundles(assetBundlePath, Application.streamingAssetsPath, platform);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            var streamingAssetsPath = Application.streamingAssetsPath;
            if (Directory.Exists(streamingAssetsPath))
            {
                Directory.Delete(streamingAssetsPath, true);
            }
            var streamingAssetsMetaPath = $"{streamingAssetsPath}.meta";
            if (File.Exists(streamingAssetsMetaPath))
            {
                File.Delete(streamingAssetsMetaPath);
            }
        }

        private void CopyAssetBundles(string sourcePath, string targetPath, KaedePlatform platform)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
            List<string> excludeFiles = new();
            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Extension == ".manifest")
                {
                    excludeFiles.Add(file);
                }

                if (fileInfo.Name == platform.ToString("G"))
                {
                    excludeFiles.Add(file);
                }
            }

            files = files.Except(excludeFiles).ToList();

            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                string relativePath = fileInfo.FullName.Substring(sourcePath.Length + 1);
                string targetFilePath = Path.Combine(targetPath, relativePath);
                string targetDirectory = Path.GetDirectoryName(targetFilePath);

                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                File.Copy(file, targetFilePath, true);
            }
        }
    }
}
