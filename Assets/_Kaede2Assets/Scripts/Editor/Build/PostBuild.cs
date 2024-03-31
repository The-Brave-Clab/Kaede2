using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace Kaede2.Editor.Build
{

    public class PostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
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
        }
    }

}