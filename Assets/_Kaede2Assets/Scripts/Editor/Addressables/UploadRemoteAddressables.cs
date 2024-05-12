using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kaede2.Editor.Addressables
{
    public static class UploadRemoteAddressables
    {
        [Preserve]
        public static string RemoteBuildPath => 
#if SCENARIO_ONLY
            Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Builds", "ServerData", "ScenarioOnly");
#else
            Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Builds", "ServerData", "Main");
#endif

        [MenuItem("Kaede2/Addressables/Upload/All", false, 0)]
        public static void UploadAll()
        {
            Upload("");
        }

        [MenuItem("Kaede2/Addressables/Upload/Windows", false, 20)]
        public static void UploadWindows()
        {
            Upload($"{SimplifiedPlatform.Windows:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/macOS", false, 21)]
        public static void UploadMacOS()
        {
            Upload($"{SimplifiedPlatform.macOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Linux", false, 22)]
        public static void UploadLinux()
        {
            Upload($"{SimplifiedPlatform.Linux:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Android", false, 23)]
        public static void UploadAndroid()
        {
            Upload($"{SimplifiedPlatform.Android:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/iOS", false, 24)]
        public static void UploadiOS()
        {
            Upload($"{SimplifiedPlatform.iOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Web", false, 25)]
        public static void UploadWeb()
        {
            Upload($"{SimplifiedPlatform.Web:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/All", true)]
        public static bool CanUploadAll()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, "");
        }

        [MenuItem("Kaede2/Addressables/Upload/Windows", true)]
        public static bool CanUploadWindows()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, $"{SimplifiedPlatform.Windows:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/macOS", true)]
        public static bool CanUploadMacOS()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, $"{SimplifiedPlatform.macOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Linux", true)]
        public static bool CanUploadLinux()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, $"{SimplifiedPlatform.Linux:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Android", true)]
        public static bool CanUploadAndroid()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, $"{SimplifiedPlatform.Android:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/iOS", true)]
        public static bool CanUploadiOS()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, $"{SimplifiedPlatform.iOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Web", true)]
        public static bool CanUploadWeb()
        {
            return AWSEditorUtils.CanUploadSubFolder(RemoteBuildPath, $"{SimplifiedPlatform.Web:G}");
        }

        private static string AdditionalPrefixVarName => "AdditionalPrefix";

        private static string GetValueByName(string varName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            return settings.profileSettings.GetValueByName(settings.activeProfileId, varName);
        }

        private static void Upload(string subFolder)
        {
            var additionalPrefix = (GetValueByName(AdditionalPrefixVarName) + "/" + subFolder.Trim('/')).Trim('/');
            AWSEditorUtils.UploadSubFolder(RemoteBuildPath, subFolder, additionalPrefix, AWS.AddressableBucket);
        }
    }
}