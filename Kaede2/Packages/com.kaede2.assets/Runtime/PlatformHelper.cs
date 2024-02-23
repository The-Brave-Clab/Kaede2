using UnityEngine;

namespace Kaede2.Assets
{
    public enum KaedePlatform
    {
        Windows,
        macOS,
        Linux,
        Android,
        iOS,
        WebGL
    }

    public static class PlatformHelper
    {
        public static KaedePlatform FromRuntimePlatform()
        {
            return FromRuntimePlatform(Application.platform);
        }

        public static KaedePlatform FromRuntimePlatform(RuntimePlatform runtimePlatform)
        {
            switch (runtimePlatform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return KaedePlatform.Windows;
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return KaedePlatform.macOS;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return KaedePlatform.Linux;
                case RuntimePlatform.Android:
                    return KaedePlatform.Android;
                case RuntimePlatform.IPhonePlayer:
                    return KaedePlatform.iOS;
                case RuntimePlatform.WebGLPlayer:
                    return KaedePlatform.WebGL;
                default:
                    return KaedePlatform.Windows;
            }
        }
    }
}