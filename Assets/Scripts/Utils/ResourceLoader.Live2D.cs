using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public class Live2DLoadRequest
        {
            public class Model
            {
                public string version { get; set; } = "";
                public string model { get; set; } = "";
                public string[] textures { get; set; } = null;

                public class MotionFile
                {
                    public string file { get; set; } = "";
                }

                public Dictionary<string, List<MotionFile>> motions { get; set; } = null;
                public string pose { get; set; } = "";
            }

            public class LoadedModel
            {
                public string name;
                public TextAsset mocFile;
                public Texture2D[] textures;
                public Dictionary<string, List<TextAsset>> motionFiles;
                public TextAsset poseFile;
            }

            private readonly string modelName;

            private LoadedModel result;
            private bool isDone;

            public Action<LoadedModel> onFinishedCallback;

            public string ModelName => modelName;
            public LoadedModel Result => result;
            public bool IsDone => isDone;

            internal Live2DLoadRequest(string modelName)
            {
                this.modelName = modelName;

                result = null;
                isDone = false;

                onFinishedCallback = null;
            }

            private Action<LoadedModel> OnFinishedCallback => t =>
            {
                result = t;
                isDone = true;
                onFinishedCallback?.Invoke(t);
            };

            public IEnumerator Send()
            {
                const string basePath = "scenario_common/live2d";

                var modelJson = Load<TextAsset>($"{basePath}/{modelName}/model.json");
                while (!modelJson.IsDone)
                {
                    yield return null;
                }

                if (modelJson.Result == null)
                {
                    Debug.LogError($"Failed to load model.json for {modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                var model = JsonConvert.DeserializeObject<Model>(modelJson.Result.text);

                if (model == null)
                {
                    Debug.LogError($"Failed to parse model.json for {modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                LoadedModel loaded = new()
                {
                    name = modelName,
                };

                CoroutineGroup loadGroup = new();

                IEnumerator WaitForAsyncOperation<T>(AsyncOperationHandle<T> handle)
                {
                    while (!handle.IsDone)
                    {
                        yield return null;
                    }
                }

                var mocFileRequest = Load<TextAsset>($"{basePath}/{modelName}/{model.model}.bytes");
                loadGroup.Add(WaitForAsyncOperation(mocFileRequest));

                loaded.textures = new Texture2D[model.textures.Length];
                var textureRequests = new AsyncOperationHandle<Texture2D>[model.textures.Length];
                for (int i = 0; i < model.textures.Length; ++i)
                {
                    textureRequests[i] = Load<Texture2D>($"{basePath}/{modelName}/{model.textures[i]}");
                    loadGroup.Add(WaitForAsyncOperation(textureRequests[i]));
                }

                loaded.motionFiles = new Dictionary<string, List<TextAsset>>();
                var motionRequests = new Dictionary<string, List<AsyncOperationHandle<TextAsset>>>();
                if (model.motions != null)
                {
                    foreach (var motion in model.motions)
                    {
                        motionRequests.Add(motion.Key, new List<AsyncOperationHandle<TextAsset>>());
                        loaded.motionFiles.Add(motion.Key, new List<TextAsset>());
                        foreach (var motionFile in motion.Value)
                        {
                            var motionRequest = Load<TextAsset>($"{basePath}/{modelName}/{motionFile.file}.bytes");
                            motionRequests[motion.Key].Add(motionRequest);
                            loadGroup.Add(WaitForAsyncOperation(motionRequest));
                        }
                    }
                }

                AsyncOperationHandle<TextAsset> poseRequest = new AsyncOperationHandle<TextAsset>();
                if (!string.IsNullOrEmpty(model.pose))
                {
                    poseRequest = Load<TextAsset>($"{basePath}/{modelName}/{model.pose}");
                    loadGroup.Add(WaitForAsyncOperation(poseRequest));
                }

                yield return loadGroup.WaitForAll();

                loaded.mocFile = mocFileRequest.Result;
                if (loaded.mocFile == null)
                {
                    Debug.LogError($"Failed to load moc file for {modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                for (int i = 0; i < model.textures.Length; ++i)
                {
                    loaded.textures[i] = textureRequests[i].Result;
                    if (loaded.textures[i] == null)
                    {
                        Debug.LogError($"Failed to load texture {model.textures[i]} for {modelName}");
                        OnFinishedCallback(null);
                        yield break;
                    }
                }

                if (model.motions != null)
                {
                    foreach (var motion in model.motions)
                    {
                        foreach (var motionFile in motion.Value)
                        {
                            var loadedMotion = motionRequests[motion.Key][motion.Value.IndexOf(motionFile)].Result;
                            if (loadedMotion == null)
                            {
                                Debug.LogError($"Failed to load motion file {motionFile.file} for {modelName}");
                                OnFinishedCallback(null);
                                yield break;
                            }
                            loaded.motionFiles[motion.Key].Add(loadedMotion);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.pose))
                {
                    loaded.poseFile = poseRequest.Result;
                    if (loaded.poseFile == null)
                    {
                        Debug.LogError($"Failed to load pose file for {modelName}");
                        OnFinishedCallback(null);
                        yield break;
                    }
                }

                OnFinishedCallback(loaded);
            }
        }
    }
}