using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Kaede2.Utils
{
    public static class SaveTexture
    {
        private static byte[] pngData;
#if UNITY_STANDALONE || UNITY_EDITOR
        private static string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
#else
        private static string directory = Path.Combine(Application.persistentDataPath, "Cache/SavedImages");
#endif
        public static void Save(string name, Texture2D texture)
        {
#if !UNITY_STANDALONE && !UNITY_EDITOR
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    File.Delete(file);
                }
            }
#endif
            RenderTexture tmp = RenderTexture.GetTemporary( 
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(texture, tmp);
    
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;

            Texture2D readableTexture = new Texture2D(texture.width, texture.height);
            readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;

            RenderTexture.ReleaseTemporary(tmp);

            var bytes = readableTexture.EncodeToPNG();

            typeof(SaveTexture).Log($"Saving texture to {name}...");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            string filePath = OpenWinSaveFileDialog("Save Texture", directory, name);
            if (filePath != null)
            {
                if (Path.GetExtension(filePath) == "")
                    filePath += ".png";
                File.WriteAllBytes(filePath, bytes);
                directory = Path.GetDirectoryName(filePath);
            }
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            pngData = bytes;
            if (Path.GetExtension(name) == "")
                name += ".png";
            IntPtr titlePtr = Marshal.StringToHGlobalAuto("Save Texture");
            IntPtr directoryPtr = Marshal.StringToHGlobalAuto(directory);
            IntPtr namePtr = Marshal.StringToHGlobalAuto(name);

            OpenMacSaveFileDialog(titlePtr, directoryPtr, namePtr, OnFileSelected);

            Marshal.FreeHGlobal(titlePtr);
            Marshal.FreeHGlobal(directoryPtr);
            Marshal.FreeHGlobal(namePtr);
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            throw new System.NotImplementedException();
#elif UNITY_ANDROID
            if (Path.GetExtension(name) == "")
                name += ".png";
            string filePath = Path.Combine(directory, name);
            File.WriteAllBytes(filePath, bytes);

            // currentActivity = UnityPlayer.currentActivity;
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            // intent = new Intent();
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

            // file = new File(filePath);
            AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", filePath);

            // uri = FileProvider.getUriForFile(currentActivity, "com.kaede2.unityshare.provider", file);
            AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");

            object[] providerParams = new object[3];
            providerParams[0] = currentActivity;
            providerParams[1] = "com.kaede2.unityshare.provider";
            providerParams[2] = fileObject;

            AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", providerParams);

            // intent.putExtra(Intent.EXTRA_STREAM, uri);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
            // intent.setType("image/png");
            intentObject.Call<AndroidJavaObject>("setType", "image/png");
            // intent.putExtra(Intent.EXTRA_SUBJECT, name);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), name);
            // intent.putExtra(Intent.EXTRA_TEXT, "");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "");

            // intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            intentObject.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));

            // chooser = Intent.createChooser(intent, name);
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, name);
            // currentActivity.startActivity(chooser);
            currentActivity.Call("startActivity", chooser);

#elif UNITY_IOS
            string filePath = Path.Combine(directory, name);
            File.WriteAllBytes(filePath, bytes);
            ShareFile(filePath);
#else
            throw new System.NotImplementedException();
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetSaveFileName([In, Out] OPENFILENAME ofn);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetActiveWindow();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OPENFILENAME
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public IntPtr lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int FlagsEx;
        }

        private static string OpenWinSaveFileDialog(string title, string directory, string defaultFileName)
        {
            IntPtr buffer = Marshal.StringToHGlobalAuto(new string('\0', 1024));
            // copy default file name to buffer
            Marshal.Copy(defaultFileName.ToCharArray(), 0, buffer, defaultFileName.Length);

            OPENFILENAME ofn = new OPENFILENAME();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.hwndOwner = GetActiveWindow();
            ofn.lpstrFile = buffer;
            ofn.lpstrInitialDir = directory;
            ofn.lpstrTitle = title;
            ofn.Flags = 0x00000002 | 0x00000800 | 0x00001000 | 0x00000008; // OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY | OFN_NOCHANGEDIR
            ofn.nMaxFile = 1024;
            ofn.lpstrFilter = "PNG Files\0*.png\0";

            if (GetSaveFileName(ofn))
            {
                string filePath = Marshal.PtrToStringAuto(ofn.lpstrFile);
                typeof(SaveTexture).Log("Selected file path: " + filePath);
                return filePath;
            }
            else
            {
                typeof(SaveTexture).Log("No file was selected or dialog was cancelled.");
            }

            return null;
        }
#endif
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [DllImport("MacSaveFileDialog")]
        private static extern void OpenMacSaveFileDialog(IntPtr title, IntPtr directory, IntPtr filename, Action<IntPtr> callback);

        [MonoPInvokeCallback(typeof(Action<IntPtr>))]
        private static void OnFileSelected(IntPtr filePathPtr)
        {
            if (filePathPtr != IntPtr.Zero)
            {
                string filePath = Marshal.PtrToStringAuto(filePathPtr);
                typeof(SaveTexture).Log("Selected file path: " + filePath);
                if (filePath != null)
                {
                    File.WriteAllBytes(filePath, pngData);
                    directory = Path.GetDirectoryName(filePath);
                }
            }
            else
            {
                typeof(SaveTexture).Log("No file was selected or dialog was cancelled.");
            }
            pngData = null;
        }
#endif
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void ShareFile(string filePath);
#endif
    }
}