using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Build
{
    public static class UploadBuilds
    {
        private static string BuildPath => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Builds");

        [MenuItem("Kaede2/Build/Upload/Web", false)]
        public static void UploadWeb()
        {
            var subFolder = $"{SimplifiedPlatform.Web:G}";
            AWSEditorUtils.UploadSubFolder(BuildPath, subFolder, subFolder, AWS.PublishBucket);
        }

        [MenuItem("Kaede2/Build/Upload/Web", true)]
        public static bool CanUploadWeb()
        {
            return AWSEditorUtils.CanUploadSubFolder(BuildPath, $"{SimplifiedPlatform.Web:G}");
        }
    }
}