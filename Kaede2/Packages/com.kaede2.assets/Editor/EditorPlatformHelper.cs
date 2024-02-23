using UnityEditor;

namespace Kaede2.Assets.Editor
{
    public static class EditorPlatformHelper
    {
        public static KaedePlatform FromBuildTarget()
        {
            return FromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
        }

        public static KaedePlatform FromBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    return KaedePlatform.Windows;
                case BuildTarget.StandaloneOSX:
                    return KaedePlatform.macOS;
                case BuildTarget.StandaloneLinux64:
                    return KaedePlatform.Linux;
                case BuildTarget.Android:
                    return KaedePlatform.Android;
                case BuildTarget.iOS:
                    return KaedePlatform.iOS;
                case BuildTarget.WebGL:
                    return KaedePlatform.WebGL;
                default:
                    return KaedePlatform.Windows;
            }
        }
    }
}