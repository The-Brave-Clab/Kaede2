using System;

// This file is auto-generated. Do not modify or move this file.

namespace SuperUnityBuild.Generated
{
    public enum ReleaseType
    {
        None,
        Debug,
        Release,
    }

    public enum Platform
    {
        None,
        PC,
        macOS,
        Linux,
        Android,
        iOS,
        WebGL,
    }

    public enum ScriptingBackend
    {
        None,
        IL2CPP,
    }

    public enum Architecture
    {
        None,
        Windows_x64,
        macOS,
        Linux_x64,
        Android,
        iOS,
        WebGL,
    }

    public enum Distribution
    {
        None,
    }

    public static class BuildConstants
    {
        public static readonly DateTime buildDate = new DateTime(638447149440598308);
        public const string version = "1.0";
        public const ReleaseType releaseType = ReleaseType.Debug;
        public const Platform platform = Platform.PC;
        public const ScriptingBackend scriptingBackend = ScriptingBackend.IL2CPP;
        public const Architecture architecture = Architecture.Windows_x64;
        public const Distribution distribution = Distribution.None;
    }
}

