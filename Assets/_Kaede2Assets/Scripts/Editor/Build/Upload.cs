using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;
using Kaede2.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.Build
{
    public static class Upload
    {
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.APNortheast1;
        private const string BucketName = "kaede2";
        private const string ProfileName = "github_actions";

        [MenuItem("Kaede2/Build/Upload Web (Full)")]
        public static void UploadWebArtifacts()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadBuildArtifacts("Web", true));
        }

        [MenuItem("Kaede2/Build/Upload Web (No StreamingAssets)")]
        public static void UploadWebArtifactsNoStreamingAssets()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadBuildArtifacts("Web", false));
        }

        [MenuItem("Kaede2/Build/Test Web (Online)")]
        public static void TestWebOnline()
        {
            Application.OpenURL($"https://{BucketName}.s3.dualstack.{BucketRegion.SystemName}.amazonaws.com/Web/index.html");
        }

        [MenuItem("Kaede2/Build/Upload Web (Full)", true)]
        [MenuItem("Kaede2/Build/Upload Web (No StreamingAssets)", true)]
        public static bool CanUploadArtifacts()
        {
            var chain = new CredentialProfileStoreChain();
            return chain.TryGetAWSCredentials(ProfileName, out _);
        }

        private static IEnumerator UploadBuildArtifacts(string buildFolder, bool withStreamingAssets)
        {
            var directoryInfo = new DirectoryInfo(Application.dataPath).Parent;
            var buildPath = new DirectoryInfo(Path.Combine(directoryInfo!.FullName, "Builds", buildFolder));

            if (!buildPath.Exists)
            {
                typeof(Upload).LogError($"Build path {buildPath} does not exist.");
                yield break;
            }

            // get all files in the build folder
            // if with no streaming assets, exclude StreamingAssets folder
            var files = buildPath.GetFiles("*", SearchOption.AllDirectories)
                    .Where(file =>
                    {
                        if (withStreamingAssets) return true;

                        var relativePath = file.FullName.Substring(buildPath.FullName.Length + 1);
                        return !relativePath.StartsWith("StreamingAssets");
                    })
                    .ToArray();

            var uploadCancelled = false;

            var progressDesc = withStreamingAssets ? "With StreamingAssets" : "Without StreamingAssets";
            var parentProgressId = Progress.Start("Uploading build artifacts to AWS",
                progressDesc,
                Progress.Options.Managed);
            Progress.IsCancellable(parentProgressId);
            Progress.RegisterCancelCallback(parentProgressId, () => uploadCancelled = true);

            try
            {
                var chain = new CredentialProfileStoreChain();
                if (!chain.TryGetAWSCredentials(ProfileName, out var credentials)) yield break;

                Dictionary<string, (string key, FileInfo file)> uploadTasks = new();

                foreach (var file in files)
                {
                    var key = file.FullName.Substring(buildPath.FullName.Length + 1)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    key = $"{buildFolder}/{key}";
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
                            UploadSingleFile(task.Value.file, task.Value.key, credentials, parentProgressId, key =>
                            {
                                finishedFileCount++;
                                Progress.SetDescription(parentProgressId,
                                    $"{progressDesc} ({finishedFileCount}/{files.Length})");
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
                        typeof(Upload).LogError($"Upload {buildFolder} to AWS cancelled.");
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
                typeof(Upload).Log($"Upload {buildFolder} to AWS completed successfully.");
        }

        private static IEnumerator UploadSingleFile(FileInfo file, string key, AWSCredentials credentials, int parentProgressId, Action<string> onFinished = null)
        {
            using var client = new AmazonS3Client(credentials, BucketRegion);
            var transferUtility = new TransferUtility(client);

            var contentType = GetContentType(file);
            var contentEncoding = GetContentEncoding(file);

            var progressId = Progress.Start("Uploading", 
                key,
                Progress.Options.Managed, parentProgressId);

            var request = new TransferUtilityUploadRequest
            {
                BucketName = BucketName,
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