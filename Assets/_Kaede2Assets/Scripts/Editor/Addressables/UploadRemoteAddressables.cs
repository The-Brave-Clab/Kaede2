using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Kaede2.Editor.Addressables
{
    public static class UploadRemoteAddressables
    {
#if SCENARIO_ONLY
        public static string RemoteBuildPath => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Builds", "ServerData", "ScenarioOnly");
#else
        public static string RemoteBuildPath => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Builds", "ServerData", "Main");
#endif

        [MenuItem("Kaede2/Addressables/Upload/All", false, 0)]
        public static void UploadAll()
        {
            UploadSubFolder("");
        }

        [MenuItem("Kaede2/Addressables/Upload/Windows", false, 20)]
        public static void UploadWindows()
        {
            UploadSubFolder($"{SimplifiedPlatform.Windows:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/macOS", false, 21)]
        public static void UploadMacOS()
        {
            UploadSubFolder($"{SimplifiedPlatform.macOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Linux", false, 22)]
        public static void UploadLinux()
        {
            UploadSubFolder($"{SimplifiedPlatform.Linux:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Android", false, 23)]
        public static void UploadAndroid()
        {
            UploadSubFolder($"{SimplifiedPlatform.Android:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/iOS", false, 24)]
        public static void UploadiOS()
        {
            UploadSubFolder($"{SimplifiedPlatform.iOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Web", false, 25)]
        public static void UploadWeb()
        {
            UploadSubFolder($"{SimplifiedPlatform.Web:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/All", true)]
        public static bool CanUploadAll()
        {
            return CanUploadSubFolder("");
        }

        [MenuItem("Kaede2/Addressables/Upload/Windows", true)]
        public static bool CanUploadWindows()
        {
            return CanUploadSubFolder($"{SimplifiedPlatform.Windows:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/macOS", true)]
        public static bool CanUploadMacOS()
        {
            return CanUploadSubFolder($"{SimplifiedPlatform.macOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Linux", true)]
        public static bool CanUploadLinux()
        {
            return CanUploadSubFolder($"{SimplifiedPlatform.Linux:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Android", true)]
        public static bool CanUploadAndroid()
        {
            return CanUploadSubFolder($"{SimplifiedPlatform.Android:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/iOS", true)]
        public static bool CanUploadiOS()
        {
            return CanUploadSubFolder($"{SimplifiedPlatform.iOS:G}");
        }

        [MenuItem("Kaede2/Addressables/Upload/Web", true)]
        public static bool CanUploadWeb()
        {
            return CanUploadSubFolder($"{SimplifiedPlatform.Web:G}");
        }

        private static string AdditionalPrefixVarName => "AdditionalPrefix";

        private static string GetValueByName(string varName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            return settings.profileSettings.GetValueByName(settings.activeProfileId, varName);
        }

        private static void UploadSubFolder(string subFolder)
        {
            var folder = Path.Combine(RemoteBuildPath, subFolder);
            var additionalPrefix = (GetValueByName(AdditionalPrefixVarName) + "/" + subFolder.Trim('/')).Trim('/');
            AWSEditorUtils.UploadFolder(folder, AWS.AddressableBucket, AWS.DefaultRegion, additionalPrefix);
        }

        private static bool CanUploadSubFolder(string subFolder)
        {
            var folder = Path.Combine(RemoteBuildPath, subFolder);
            if (!Directory.Exists(folder)) return false;
            if (Directory.GetDirectories(folder).Length == 0) return false;
            if (!AWSEditorUtils.ValidateProfile(AWS.EditorProfileName)) return false;
            return true;
        }
    }
}