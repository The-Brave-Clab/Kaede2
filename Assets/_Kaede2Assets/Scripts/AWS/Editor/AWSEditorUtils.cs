using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Transfer;
using Kaede2.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace Kaede2.AWS.Editor
{
    public static class AWSEditorUtils
    {
        public static void UploadFolder(string folderPath, string bucket, RegionEndpoint region)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            files = files.Where(f => !f.Name.Equals(".DS_Store")).ToArray();
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadFilesCoroutine(files, bucket, region, f => f[(directoryInfo.FullName.Length + 1)..].Replace(Path.DirectorySeparatorChar, '/')));
        }

        public static void UploadFiles(FileInfo[] files, string bucket, RegionEndpoint region, Func<string, string> getKeyFromFile)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadFilesCoroutine(files, bucket, region, getKeyFromFile));
        }

        public static bool ValidateProfile(string profileName)
        {
            var chain = new CredentialProfileStoreChain();
            return chain.TryGetAWSCredentials(profileName, out _);
        }

        private static IEnumerator UploadFilesCoroutine(FileInfo[] files, string bucket, RegionEndpoint region, Func<string, string> getKeyFromFile)
        {
            var uploadCancelled = false;

            var parentProgressId = Progress.Start("Uploading files to AWS",
                $"0/{files.Length}",
                Progress.Options.Managed);
            Progress.IsCancellable(parentProgressId);
            Progress.RegisterCancelCallback(parentProgressId, () => uploadCancelled = true);

            try
            {
                var chain = new CredentialProfileStoreChain();
                if (!chain.TryGetAWSCredentials(Config.EditorProfileName, out var credentials)) yield break;

                Dictionary<string, (string key, FileInfo file)> uploadTasks = new();

                foreach (var file in files)
                {
                    var key = getKeyFromFile(file.FullName);
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
                            UploadSingleFile(task.Value.file, task.Value.key, bucket, region, credentials, parentProgressId, key =>
                            {
                                finishedFileCount++;
                                Progress.SetDescription(parentProgressId,
                                    $"{finishedFileCount}/{files.Length}");
                                Progress.Report(parentProgressId, (float)finishedFileCount / files.Length);
                                uploadCoroutines.Remove(key);
                            })
                        );
                    }
                    else
                    {
                        yield return null;
                    }

                    if (uploadCancelled)
                    {
                        typeof(AWSEditorUtils).LogError("Upload files to AWS cancelled.");
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
                typeof(AWSEditorUtils).Log("Upload files to AWS completed successfully.");
        }

        private static IEnumerator UploadSingleFile(FileInfo file, string key, string bucket, RegionEndpoint region, AWSCredentials credentials, int parentProgressId, Action<string> onFinished = null)
        {
            using var client = new AmazonS3Client(credentials, region);
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

            onFinished?.Invoke(key);
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
    }
}