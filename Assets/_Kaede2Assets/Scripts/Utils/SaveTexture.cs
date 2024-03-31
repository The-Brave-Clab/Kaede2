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
            throw new System.NotImplementedException();
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            pngData = bytes;
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
            throw new System.NotImplementedException();
#elif UNITY_IOS
            string filePath = Path.Combine(directory, name);
            File.WriteAllBytes(filePath, bytes);
            ShareFile(filePath);
#else
            throw new System.NotImplementedException();
#endif
        }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [DllImport("MacSaveFileDialog")]
        static extern void OpenMacSaveFileDialog(IntPtr title, IntPtr directory, IntPtr filename, Action<IntPtr> callback);

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