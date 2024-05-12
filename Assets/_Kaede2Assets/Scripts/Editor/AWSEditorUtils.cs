using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Transfer;
using Kaede2.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor
{
    public static class AWSEditorUtils
    {
        private static string UploadHistoryPath => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "AWSUploadHistory.json");
        private static UploadHistoryJson uploadHistory;

        static void ResetUploadHistory()
        {
            uploadHistory = File.Exists(UploadHistoryPath) ?
                JsonUtility.FromJson<UploadHistoryJson>(File.ReadAllText(UploadHistoryPath)) :
                new UploadHistoryJson { history = new List<UploadFileInfo>() };
        }

        private static void SaveHistory()
        {
            File.WriteAllText(UploadHistoryPath, JsonUtility.ToJson(uploadHistory, true));
        }

        public static void UploadFolder(string folderPath, string bucket, RegionEndpoint region, string additionalPrefix = "")
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            // order by descending so that the largest files are uploaded first
            // this will save some time
            files = files.Where(f => !f.Name.Equals(".DS_Store")).OrderByDescending(f => f.Length).ToArray();
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadFilesCoroutine(files, bucket, region, additionalPrefix, f => f[(directoryInfo.FullName.Length + 1)..].Replace(Path.DirectorySeparatorChar, '/')));
        }

        public static void UploadFiles(FileInfo[] files, string bucket, RegionEndpoint region, Func<string, string> getKeyFromFile, string additionalPrefix = "")
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadFilesCoroutine(files, bucket, region, additionalPrefix, getKeyFromFile));
        }

        public static bool ValidateProfile(string profileName)
        {
            var chain = new CredentialProfileStoreChain();
            return chain.TryGetAWSCredentials(profileName, out _);
        }

        private static IEnumerator UploadFilesCoroutine(FileInfo[] files, string bucket, RegionEndpoint region, string additionalPrefix, Func<string, string> getKeyFromFile)
        {
            int filesActuallyUploaded = 0;
            var uploadCancelled = false;

            var parentProgressId = Progress.Start($"Uploading files to AWS S3 ({bucket})",
                $"0/{files.Length}",
                Progress.Options.Managed);
            Progress.IsCancellable(parentProgressId);
            Progress.RegisterCancelCallback(parentProgressId, () => uploadCancelled = true);

            ResetUploadHistory();

            try
            {
                var chain = new CredentialProfileStoreChain();
                if (!chain.TryGetAWSCredentials(AWS.EditorProfileName, out var credentials)) yield break;

                Dictionary<string, (string key, FileInfo file)> uploadTasks = new();

                foreach (var file in files)
                {
                    var key = getKeyFromFile(file.FullName);
                    if (!string.IsNullOrEmpty(additionalPrefix))
                    {
                        var prefix = additionalPrefix.Trim('/');
                        key = $"{prefix}/{key}";
                    }
                    uploadTasks[key] = new() { key = key, file = file };
                }

                Dictionary<string, EditorCoroutine> uploadCoroutines = new();
                const int taskCount = 16;

                int finishedFileCount = 0;
                while (uploadTasks.Count > 0 || uploadCoroutines.Count > 0)
                {
                    if (uploadCoroutines.Count < taskCount)
                    {
                        if (uploadTasks.Count == 0)
                        {
                            yield return null;
                            continue;
                        }
                        var task = uploadTasks.First();
                        uploadTasks.Remove(task.Key);
                        uploadCoroutines[task.Key] = EditorCoroutineUtility.StartCoroutineOwnerless(
                            UploadSingleFile(task.Value.file, task.Value.key, bucket, region, credentials, parentProgressId, (key, uploaded) =>
                            {
                                finishedFileCount++;
                                Progress.SetDescription(parentProgressId,
                                    $"{finishedFileCount}/{files.Length}");
                                Progress.Report(parentProgressId, (float)finishedFileCount / files.Length);
                                uploadCoroutines.Remove(key);
                                if (uploaded) filesActuallyUploaded++;
                            })
                        );
                    }
                    else
                    {
                        yield return null;
                    }

                    if (uploadCancelled)
                    {
                        typeof(AWSEditorUtils).LogError($"Upload files to AWS cancelled. {filesActuallyUploaded} files uploaded.");
                        break;
                    }
                }

                while (uploadCoroutines.Count > 0)
                {
                    yield return null;
                }

                Progress.Report(parentProgressId, 1f);
            }
            finally
            {
                Progress.Remove(parentProgressId);
            }

            if (!uploadCancelled)
                typeof(AWSEditorUtils).Log($"Upload files to AWS completed successfully. {filesActuallyUploaded} files uploaded.");
        }

        private static IEnumerator UploadSingleFile(FileInfo file, string key, string bucket, RegionEndpoint region, AWSCredentials credentials, int parentProgressId, Action<string, bool> onFinished = null)
        {
            bool needUpload = true;
            int historyEntryIndex = -1;
            // UploadFileInfo currentFile = UploadFileInfo.GetFromFile(file, bucket, key);
            // launch UploadFileInfo.GetFromFile in a Task since it will take a while
            // wait inside the coroutine
            Task<UploadFileInfo> task = Task.Run(() => UploadFileInfo.GetFromFile(file, bucket, key));
            yield return new WaitUntil(() => task.IsCompleted);
            UploadFileInfo currentFile = task.Result;
            
            for (int i = 0; i < uploadHistory.history.Count; i++)
            {
                UploadFileInfo historyEntry = uploadHistory.history[i];
                if (historyEntry.key != currentFile.key || historyEntry.bucket != currentFile.bucket) continue;
                needUpload = historyEntry.md5 != currentFile.md5 || historyEntry.sha256 != currentFile.sha256;
                historyEntryIndex = i;
                break;
            }
            if (!needUpload)
            {
                onFinished?.Invoke(key, false);
                yield break;
            }

            AmazonS3Config config = new AmazonS3Config
            {
                UseDualstackEndpoint = true,
                UseAccelerateEndpoint = true,
                RegionEndpoint = AWS.DefaultRegion,
            };
            using var client = new AmazonS3Client(credentials, config);
            var transferUtility = new TransferUtility(client);

            var contentType = GetContentType(file);
            var contentEncoding = GetContentEncoding(file);

            var progressId = Progress.Start("Uploading", 
                key,
                Progress.Options.Managed, parentProgressId);

            var request = new TransferUtilityUploadRequest
            {
                BucketName = bucket,
                Key = key,
                FilePath = file.FullName,
            };

            if (!string.IsNullOrEmpty(contentType))
                request.ContentType = contentType;

            if (!string.IsNullOrEmpty(contentEncoding))
                request.Headers.ContentEncoding = GetContentEncoding(file);

            request.UploadProgressEvent += (_, args) =>
            {
                Progress.Report(progressId, (float) args.TransferredBytes / args.TotalBytes);
            };

            var uploadTask = transferUtility.UploadAsync(request);
            while (!uploadTask.IsCompleted)
                yield return null;

            Progress.Remove(progressId);

            if (uploadTask.Exception != null)
            {
                typeof(AWSEditorUtils).LogError($"Failed to upload {file.FullName} to {bucket}/{key}: {uploadTask.Exception}");
                onFinished?.Invoke(key, false);
                yield break;
            }

            if (historyEntryIndex < 0)
                uploadHistory.history.Add(currentFile);
            else
                uploadHistory.history[historyEntryIndex] = currentFile;

            SaveHistory();

            onFinished?.Invoke(key, true);
        }

        private static readonly Dictionary<string, string> ContentTypes = new()
        {
            { ".js", "application/javascript" },
            { ".wasm", "application/wasm" },
            { ".json", "application/json" },
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".xml", "text/xml" },
            { ".hash", "text/plain" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".ico", "image/x-icon" },
            { ".data", "binary/octet-stream" },
            { ".bundle", "binary/octet-stream" },
            { ".bin", "binary/octet-stream" },
        };
        
        private static readonly Dictionary<string, string> ContentEncodings = new()
        {
            { ".br", "br" }
        };

        private static string GetContentType(FileInfo file)
        {
            var extension = file.Extension.ToLower();
            if (ContentEncodings.ContainsKey(extension))
                extension = Path.GetExtension(Path.GetFileNameWithoutExtension(file.FullName)).ToLower();

            return ContentTypes.GetValueOrDefault(extension);
        }

        private static string GetContentEncoding(FileInfo file)
        {
            var extension = file.Extension.ToLower();
            return ContentEncodings.GetValueOrDefault(extension);
        }

        public static void UploadSubFolder(string baseFolder, string subFolder, string additionalPrefix, string bucket)
        {
            var folder = Path.Combine(baseFolder, subFolder);
            UploadFolder(folder, bucket, AWS.DefaultRegion, additionalPrefix);
        }

        public static bool CanUploadSubFolder(string baseFolder, string subFolder)
        {
            var folder = Path.Combine(baseFolder, subFolder);
            if (!Directory.Exists(folder)) return false;
            if (Directory.GetDirectories(folder).Length == 0) return false;
            if (!ValidateProfile(AWS.EditorProfileName)) return false;
            return true;
        }

        [Serializable]
        private struct UploadHistoryJson
        {
            public List<UploadFileInfo> history;
        }

        [Serializable]
        private struct UploadFileInfo : IEquatable<UploadFileInfo>
        {
            public string bucket;
            public string key;
            public string md5;
            public string sha256;

            public static UploadFileInfo GetFromFile(FileInfo file, string bucket, string key)
            {
                byte[] md5;
                byte[] sha256;
                using (var stream = file.OpenRead())
                using (var mySHA256 = SHA256.Create())
                using (var myMD5 = MD5.Create())
                {
                    stream.Position = 0;
                    md5 = myMD5.ComputeHash(stream);
                    stream.Position = 0;
                    sha256 = mySHA256.ComputeHash(stream);
                }
                return new UploadFileInfo
                {
                    bucket = bucket,
                    key = key,
                    md5 = BitConverter.ToString(md5).Replace("-", "").ToLower(),
                    sha256 = BitConverter.ToString(sha256).Replace("-", "").ToLower(),
                };
            }

            public bool Equals(UploadFileInfo other)
            {
                return bucket == other.bucket && key == other.key && md5 == other.md5 && sha256 == other.sha256;
            }

            public override bool Equals(object obj)
            {
                return obj is UploadFileInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(bucket, key, md5, sha256);
            }
        }
    }
}