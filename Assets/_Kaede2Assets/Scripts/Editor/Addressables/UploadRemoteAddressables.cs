using System.IO;
using Kaede2.AWS;
using Kaede2.AWS.Editor;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Addressables
{
    public static class UploadRemoteAddressables
    {
        public static string RemoteBuildPath => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "ServerData");

        [MenuItem("Kaede2/Addressables/Upload")]
        public static void Upload()
        {
            AWSEditorUtils.UploadFolder(RemoteBuildPath, Config.AddressableBucket, Config.DefaultRegion);
        }

        [MenuItem("Kaede2/Addressables/Upload", true)]
        public static bool CanUpload()
        {
            if (!Directory.Exists(RemoteBuildPath)) return false;
            if (Directory.GetDirectories(RemoteBuildPath).Length == 0) return false;
            if (!AWSEditorUtils.ValidateProfile(Config.EditorProfileName)) return false;
            return true;
        }
    }
}