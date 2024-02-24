using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaede2.Utils
{
    public partial class ResourceLoader
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
            private readonly ResourceLoader loader;

            private LoadedModel result;
            private bool isDone;

            public Action<LoadedModel> onFinishedCallback;

            public string ModelName => modelName;
            public LoadedModel Result => result;
            public bool IsDone => isDone;

            internal Live2DLoadRequest(string modelName, ResourceLoader loader)
            {
                this.modelName = modelName;
                this.loader = loader;

                result = null;
                isDone = false;

                onFinishedCallback = null;
            }

            internal Action<LoadedModel> OnFinishedCallback => t =>
            {
                result = t;
                isDone = true;
                onFinishedCallback?.Invoke(t);
            };

            public IEnumerator Send()
            {
                const string basePath = "scenario_common/live2d";

                var modelJson = loader.Load<TextAsset>($"{basePath}/{modelName}/model.json");
                yield return modelJson.Send();

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

                var mocFileRequest = loader.Load<TextAsset>($"{basePath}/{modelName}/{model.model}.bytes");
                loadGroup.Add(mocFileRequest.Send());

                loaded.textures = new Texture2D[model.textures.Length];
                var textureRequests = new Request<Texture2D>[model.textures.Length];
                for (int i = 0; i < model.textures.Length; ++i)
                {
                    textureRequests[i] = loader.Load<Texture2D>($"{basePath}/{modelName}/{model.textures[i]}");
                    loadGroup.Add(textureRequests[i].Send());
                }

                loaded.motionFiles = new Dictionary<string, List<TextAsset>>();
                var motionRequests = new Dictionary<string, List<Request<TextAsset>>>();
                if (model.motions != null)
                {
                    foreach (var motion in model.motions)
                    {
                        motionRequests.Add(motion.Key, new List<Request<TextAsset>>());
                        loaded.motionFiles.Add(motion.Key, new List<TextAsset>());
                        foreach (var motionFile in motion.Value)
                        {
                            var motionRequest = loader.Load<TextAsset>($"{basePath}/{modelName}/{motionFile.file}.bytes");
                            motionRequests[motion.Key].Add(motionRequest);
                            loadGroup.Add(motionRequest.Send());
                        }
                    }
                }

                Request<TextAsset> poseRequest = null;
                if (!string.IsNullOrEmpty(model.pose))
                {
                    poseRequest = loader.Load<TextAsset>($"{basePath}/{modelName}/{model.pose}");
                    loadGroup.Add(poseRequest.Send());
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

                if (!string.IsNullOrEmpty(model.pose) && poseRequest != null)
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