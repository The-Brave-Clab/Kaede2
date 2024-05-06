using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor
{
    public enum SimplifiedPlatform
    {
        Windows,
        macOS,
        Linux,
        Android,
        iOS,
        Web,
    }

    public static class SimplifiedBuildTarget
    {
        public static SimplifiedPlatform currentActive
        {
            get
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        return SimplifiedPlatform.Windows;
                    case BuildTarget.StandaloneOSX:
                        return SimplifiedPlatform.macOS;
                    case BuildTarget.StandaloneLinux64:
                        return SimplifiedPlatform.Linux;
                    case BuildTarget.Android:
                        return SimplifiedPlatform.Android;
                    case BuildTarget.iOS:
                        return SimplifiedPlatform.iOS;
                    case BuildTarget.WebGL:
                        return SimplifiedPlatform.Web;
                    default:
                        return SimplifiedPlatform.Windows;
                }
            }
        }

        private static string PlatformSaveFileName => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Library", "Kaede2BuildTarget.txt");
        public static void Save()
        {
            File.WriteAllText(PlatformSaveFileName, $"{currentActive:G}");
        }

        public static SimplifiedPlatform Load()
        {
            if (File.Exists(PlatformSaveFileName) && Enum.TryParse<SimplifiedPlatform>(File.ReadAllText(PlatformSaveFileName), out var platform))
                return platform;

            Save();
            return currentActive;
        }
    }
}