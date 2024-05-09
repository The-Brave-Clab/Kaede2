using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaede2.Editor.Build
{
    public class PreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            PlayerSettings.SetIl2CppStacktraceInformation(NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup), Il2CppStacktraceInformation.MethodFileLineNumber);
        }
    }
}