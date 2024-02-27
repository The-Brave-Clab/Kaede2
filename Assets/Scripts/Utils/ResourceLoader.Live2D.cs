using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public class Live2DLoadRequest
        {
            // ReSharper disable InconsistentNaming
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
            // ReSharper restore InconsistentNaming

            public class LoadedModel
            {
                public string Name;
                public TextAsset MocFile;
                public Texture2D[] Textures;
                public Dictionary<string, List<TextAsset>> MotionFiles;
                public TextAsset PoseFile;
            }

            private readonly string _modelName;

            private LoadedModel _result;
            private bool _isDone;
            private readonly List<AsyncOperationHandle> _handles;

            public string ModelName => _modelName;
            public LoadedModel Result => _result;
            public bool IsDone => _isDone;

            internal Live2DLoadRequest(string modelName)
            {
                this._modelName = modelName;

                _result = null;
                _isDone = false;
                _handles = new List<AsyncOperationHandle>();
            }

            private Action<LoadedModel> OnFinishedCallback => t =>
            {
                _result = t;
                _isDone = true;
            };

            public IEnumerator Send()
            {
                const string basePath = "scenario_common/live2d";

                var modelJson = Load<TextAsset>($"{basePath}/{_modelName}/model.json");
                _handles.Add(modelJson);
                while (!modelJson.IsDone)
                {
                    yield return null;
                }

                if (modelJson.Result == null)
                {
                    Debug.LogError($"Failed to load model.json for {_modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                var model = JsonConvert.DeserializeObject<Model>(modelJson.Result.text);

                if (model == null)
                {
                    Debug.LogError($"Failed to parse model.json for {_modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                LoadedModel loaded = new()
                {
                    Name = _modelName,
                };

                CoroutineGroup loadGroup = new();

                IEnumerator WaitForAsyncOperation<T>(AsyncOperationHandle<T> handle)
                {
                    while (!handle.IsDone)
                    {
                        yield return null;
                    }
                }

                var mocFileRequest = Load<TextAsset>($"{basePath}/{_modelName}/{model.model}.bytes");
                _handles.Add(mocFileRequest);
                loadGroup.Add(WaitForAsyncOperation(mocFileRequest));

                loaded.Textures = new Texture2D[model.textures.Length];
                var textureRequests = new AsyncOperationHandle<Texture2D>[model.textures.Length];
                for (int i = 0; i < model.textures.Length; ++i)
                {
                    textureRequests[i] = Load<Texture2D>($"{basePath}/{_modelName}/{model.textures[i]}");
                    _handles.Add(textureRequests[i]);
                    loadGroup.Add(WaitForAsyncOperation(textureRequests[i]));
                }

                loaded.MotionFiles = new Dictionary<string, List<TextAsset>>();
                var motionRequests = new Dictionary<string, List<AsyncOperationHandle<TextAsset>>>();
                if (model.motions != null)
                {
                    foreach (var motion in model.motions)
                    {
                        motionRequests.Add(motion.Key, new List<AsyncOperationHandle<TextAsset>>());
                        loaded.MotionFiles.Add(motion.Key, new List<TextAsset>());
                        foreach (var motionFile in motion.Value)
                        {
                            var motionRequest = Load<TextAsset>($"{basePath}/{_modelName}/{motionFile.file}.bytes");
                            _handles.Add(motionRequest);
                            motionRequests[motion.Key].Add(motionRequest);
                            loadGroup.Add(WaitForAsyncOperation(motionRequest));
                        }
                    }
                }

                AsyncOperationHandle<TextAsset> poseRequest = new AsyncOperationHandle<TextAsset>();
                if (!string.IsNullOrEmpty(model.pose))
                {
                    poseRequest = Load<TextAsset>($"{basePath}/{_modelName}/{model.pose}");
                    _handles.Add(poseRequest);
                    loadGroup.Add(WaitForAsyncOperation(poseRequest));
                }

                yield return loadGroup.WaitForAll();

                loaded.MocFile = mocFileRequest.Result;
                if (loaded.MocFile == null)
                {
                    Debug.LogError($"Failed to load moc file for {_modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                for (int i = 0; i < model.textures.Length; ++i)
                {
                    loaded.Textures[i] = textureRequests[i].Result;
                    if (loaded.Textures[i] == null)
                    {
                        Debug.LogError($"Failed to load texture {model.textures[i]} for {_modelName}");
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
                                Debug.LogError($"Failed to load motion file {motionFile.file} for {_modelName}");
                                OnFinishedCallback(null);
                                yield break;
                            }
                            loaded.MotionFiles[motion.Key].Add(loadedMotion);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.pose))
                {
                    loaded.PoseFile = poseRequest.Result;
                    if (loaded.PoseFile == null)
                    {
                        Debug.LogError($"Failed to load pose file for {_modelName}");
                        OnFinishedCallback(null);
                        yield break;
                    }
                }

                OnFinishedCallback(loaded);
            }

            public void Release()
            {
                foreach (var handle in _handles)
                {
                    Addressables.Release(handle);
                }
            }
        }
    }
}