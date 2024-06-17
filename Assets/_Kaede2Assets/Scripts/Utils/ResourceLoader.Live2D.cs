using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kaede2.Scenario.Framework.Live2D;
using Kaede2.Scenario.Framework.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public class Live2DOperation : AsyncOperationBase<Live2DAssets>
        {
            private readonly string modelName;
            private readonly List<AsyncOperationHandle> handles;

            public string ModelName => modelName;

            private Coroutine loadCoroutine;

            internal Live2DOperation(string modelName)
            {
                this.modelName = modelName;
                handles = new();
                loadCoroutine = null;
            }

            protected override void Execute()
            {
                if (string.IsNullOrEmpty(modelName))
                {
                    Complete(null, false, "Model name is empty");
                }

                if (loadCoroutine != null)
                {
                    Complete(null, false, "Operation is already running");
                }

                loadCoroutine = CoroutineProxy.Start(LoadCoroutine());
            }

            protected override void Destroy()
            {
                if (loadCoroutine != null)
                {
                    CoroutineProxy.Stop(loadCoroutine);
                    loadCoroutine = null;
                }

                foreach (var handle in handles)
                    Addressables.Release(handle);

                base.Destroy();
            }

            private IEnumerator LoadCoroutine()
            {
                const string basePath = "scenario_common/live2d";

                var modelJson = Addressables.LoadAssetAsync<TextAsset>($"{basePath}/{modelName}/model.json");
                handles.Add(modelJson);
                yield return modelJson;

                if (modelJson.Status != AsyncOperationStatus.Succeeded)
                {
                    Complete(null, false, $"Failed to load model.json for {modelName}");
                    loadCoroutine = null;
                    yield break;
                }

                var model = Live2DModelJson.FromJson(modelJson.Result.text);

                if (model == null)
                {
                    Complete(null, false, $"Failed to parse model.json for {modelName}");
                    loadCoroutine = null;
                    yield break;
                }

                var loaded = ScriptableObject.CreateInstance<Live2DAssets>();
                loaded.name = modelName;
                loaded.modelName = modelName;

                CoroutineGroup loadGroup = new();

                var mocFileRequest = Addressables.LoadAssetAsync<TextAsset>($"{basePath}/{modelName}/{model.model}.bytes");
                handles.Add(mocFileRequest);
                loadGroup.Add(mocFileRequest);

                loaded.textures = new Texture2D[model.textures.Length];
                var textureRequests = new AsyncOperationHandle<Texture2D>[model.textures.Length];
                for (int i = 0; i < model.textures.Length; ++i)
                {
                    textureRequests[i] = Addressables.LoadAssetAsync<Texture2D>($"{basePath}/{modelName}/{model.textures[i]}");
                    handles.Add(textureRequests[i]);
                    loadGroup.Add(textureRequests[i]);
                }

                loaded.motionFiles = new();
                var motionRequests = new Dictionary<string, List<AsyncOperationHandle<TextAsset>>>();
                if (model.motions != null)
                {
                    foreach (var (motionName, motionFiles) in model.motions)
                    {
                        motionRequests.Add(motionName, new List<AsyncOperationHandle<TextAsset>>());
                        loaded.motionFiles.Add(new()
                        {
                            name = motionName,
                            files = new List<TextAsset>()
                        });
                        foreach (var motionFile in motionFiles)
                        {
                            var motionRequest = Addressables.LoadAssetAsync<TextAsset>($"{basePath}/{modelName}/{motionFile.file}.bytes");
                            handles.Add(motionRequest);
                            motionRequests[motionName].Add(motionRequest);
                            loadGroup.Add(motionRequest);
                        }
                    }
                }

                AsyncOperationHandle<TextAsset> poseRequest = new();
                if (!string.IsNullOrEmpty(model.pose))
                {
                    poseRequest = Addressables.LoadAssetAsync<TextAsset>($"{basePath}/{modelName}/{model.pose}");
                    handles.Add(poseRequest);
                    loadGroup.Add(poseRequest);
                }

                yield return loadGroup.WaitForAll();

                loaded.mocFile = mocFileRequest.Result;
                if (loaded.mocFile == null)
                {
                    Complete(null, false, $"Failed to load moc file for {modelName}");
                    loadCoroutine = null;
                    yield break;
                }

                for (int i = 0; i < model.textures.Length; ++i)
                {
                    loaded.textures[i] = textureRequests[i].Result;
                    if (loaded.textures[i] == null)
                    {
                        Complete(null, false, $"Failed to load texture {model.textures[i]} for {modelName}");
                        loadCoroutine = null;
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
                                Complete(null, false, $"Failed to load motion file {motionFiles[i].file} for {modelName}");
                                loadCoroutine = null;
                                yield break;
                            }

                            loaded.GetMotionFile(motionName).files.Add(loadedMotion);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(model.pose) && poseRequest.IsValid())
                {
                    loaded.poseFile = poseRequest.Result;
                    if (loaded.poseFile == null)
                    {
                        Complete(null, false, $"Failed to load pose file for {modelName}");
                        loadCoroutine = null;
                        yield break;
                    }
                }

                Complete(loaded, true, "");
                loadCoroutine = null;
            }
        }
    }
}