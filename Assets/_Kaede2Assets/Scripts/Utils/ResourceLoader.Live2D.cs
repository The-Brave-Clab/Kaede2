using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kaede2.Scenario.Framework.Live2D;
using Kaede2.Scenario.Framework.Utils;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public class LoadLive2DHandle : BaseHandle<Live2DAssets>
        {
            private readonly string modelName;
            private readonly List<HandleBase> handles;

            public string ModelName => modelName;

            internal LoadLive2DHandle(string modelName)
            {
                this.modelName = modelName;

                result = null;
                isDone = false;
                handles = new List<HandleBase>();
            }

            private void OnFinishedCallback(Live2DAssets t)
            {
                progress = 1;
                isDone = true;
                status = t == null ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
                result = t;
            }

            public override IEnumerator Send()
            {
                const string basePath = "scenario_common/live2d";

                var modelJson = Load<TextAsset>($"{basePath}/{modelName}/model.json");
                handles.Add(modelJson);
                yield return modelJson.Send();

                if (modelJson.Result == null)
                {
                    Debug.LogError($"Failed to load model.json for {modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                var model = Live2DModelJson.FromJson(modelJson.Result.text);

                if (model == null)
                {
                    Debug.LogError($"Failed to parse model.json for {modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                var loaded = ScriptableObject.CreateInstance<Live2DAssets>();
                loaded.name = modelName;
                loaded.modelName = modelName;

                CoroutineGroup loadGroup = new();

                var mocFileRequest = Load<TextAsset>($"{basePath}/{modelName}/{model.model}.bytes");
                handles.Add(mocFileRequest);
                loadGroup.Add(mocFileRequest.Send());

                loaded.textures = new Texture2D[model.textures.Length];
                var textureRequests = new LoadAddressableHandle<Texture2D>[model.textures.Length];
                for (int i = 0; i < model.textures.Length; ++i)
                {
                    textureRequests[i] = Load<Texture2D>($"{basePath}/{modelName}/{model.textures[i]}");
                    handles.Add(textureRequests[i]);
                    loadGroup.Add(textureRequests[i].Send());
                }

                loaded.motionFiles = new();
                var motionRequests = new Dictionary<string, List<LoadAddressableHandle<TextAsset>>>();
                if (model.motions != null)
                {
                    foreach (var (motionName, motionFiles) in model.motions)
                    {
                        motionRequests.Add(motionName, new List<LoadAddressableHandle<TextAsset>>());
                        loaded.motionFiles.Add(new()
                        {
                            name = motionName,
                            files = new List<TextAsset>()
                        });
                        foreach (var motionFile in motionFiles)
                        {
                            var motionRequest = Load<TextAsset>($"{basePath}/{modelName}/{motionFile.file}.bytes");
                            handles.Add(motionRequest);
                            motionRequests[motionName].Add(motionRequest);
                            loadGroup.Add(motionRequest.Send());
                        }
                    }
                }

                LoadAddressableHandle<TextAsset> poseRequest = null;
                if (!string.IsNullOrEmpty(model.pose))
                {
                    poseRequest = Load<TextAsset>($"{basePath}/{modelName}/{model.pose}");
                    handles.Add(poseRequest);
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
                    foreach (var (motionName, motionFiles) in model.motions)
                    {
                        for (var i = 0; i < motionFiles.Count; i++)
                        {
                            var loadedMotion = motionRequests[motionName][i].Result;
                            if (loadedMotion == null)
                            {
                                Debug.LogError($"Failed to load motion file {motionFiles[i].file} for {modelName}");
                                OnFinishedCallback(null);
                                yield break;
                            }

                            loaded.GetMotionFile(motionName).files.Add(loadedMotion);
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

            public override void Dispose()
            {
                foreach (var handle in handles)
                {
                    handle.Dispose();
                }
                base.Dispose();
            }
        }
    }
}