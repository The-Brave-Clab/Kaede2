using UnityEditor;

namespace Kaede2.Editor
{
    public enum SimplifiedPlatform
    {
        Windows,
        macOS,
        Linux,
        Android,
        iOS,
        WebGL,
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
                        return SimplifiedPlatform.WebGL;
                    default:
                        return SimplifiedPlatform.Windows;
                }
            }
        }
    }
}