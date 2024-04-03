using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Kaede2.Editor.Build
{
    public class PreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL)
            {
                const string ASYNCIFY_ARG = "-sASYNCIFY=1";
                if (string.IsNullOrEmpty(PlayerSettings.WebGL.emscriptenArgs))
                {
                    PlayerSettings.WebGL.emscriptenArgs = ASYNCIFY_ARG;
                }
                else if (!PlayerSettings.WebGL.emscriptenArgs.Contains(ASYNCIFY_ARG))
                {
                    PlayerSettings.WebGL.emscriptenArgs += " " + ASYNCIFY_ARG;
                }
            }
        }
    }
}