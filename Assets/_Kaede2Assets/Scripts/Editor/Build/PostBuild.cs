using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
#endif

namespace Kaede2.Editor.Build
{
    public class PostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
#if UNITY_EDITOR_OSX
            if (report.summary.platform == BuildTarget.iOS)
            {
                string plistPath = report.summary.outputPath + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                
                // Add NSPhotoLibraryAddUsageDescription to Info.plist
                rootDict.SetString("NSPhotoLibraryAddUsageDescription",
                    $"{PlayerSettings.productName} needs permission to save photos to your photo library.");

                File.WriteAllText(plistPath, plist.WriteToString());
            }
#endif
        }
    }

}