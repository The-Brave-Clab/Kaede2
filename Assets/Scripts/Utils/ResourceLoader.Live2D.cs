using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kaede2.Live2D;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2.Utils
{
    public static partial class ResourceLoader
    {
        public class LoadLive2DHandle : BaseHandle<Live2DAssets>
        {
            private readonly string _modelName;
            private readonly List<HandleBase> _handles;

            public string ModelName => _modelName;

            internal LoadLive2DHandle(string modelName)
            {
                _modelName = modelName;

                _result = null;
                _isDone = false;
                _handles = new List<HandleBase>();
            }

            private void OnFinishedCallback(Live2DAssets t)
            {
                _progress = 1;
                _isDone = true;
                _status = t == null ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
                _result = t;
            }

            public override IEnumerator Send()
            {
                const string basePath = "scenario_common/live2d";

                var modelJson = Load<TextAsset>($"{basePath}/{_modelName}/model.json");
                _handles.Add(modelJson);
                yield return modelJson.Send();

                if (modelJson.Result == null)
                {
                    Debug.LogError($"Failed to load model.json for {_modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                var model = Live2DModelJson.FromJson(modelJson.Result.text);

                if (model == null)
                {
                    Debug.LogError($"Failed to parse model.json for {_modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                var loaded = ScriptableObject.CreateInstance<Live2DAssets>();
                loaded.name = _modelName;
                loaded.modelName = _modelName;

                CoroutineGroup loadGroup = new();

                IEnumerator WaitForAsyncOperation(IEnumerator operation)
                {
                    while (operation.MoveNext())
                    {
                        yield return operation.Current;
                    }
                }

                var mocFileRequest = Load<TextAsset>($"{basePath}/{_modelName}/{model.model}.bytes");
                _handles.Add(mocFileRequest);
                loadGroup.Add(WaitForAsyncOperation(mocFileRequest.Send()));

                loaded.textures = new Texture2D[model.textures.Length];
                var textureRequests = new LoadAddressableHandle<Texture2D>[model.textures.Length];
                for (int i = 0; i < model.textures.Length; ++i)
                {
                    textureRequests[i] = Load<Texture2D>($"{basePath}/{_modelName}/{model.textures[i]}");
                    _handles.Add(textureRequests[i]);
                    loadGroup.Add(WaitForAsyncOperation(textureRequests[i].Send()));
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
                            var motionRequest = Load<TextAsset>($"{basePath}/{_modelName}/{motionFile.file}.bytes");
                            _handles.Add(motionRequest);
                            motionRequests[motionName].Add(motionRequest);
                            loadGroup.Add(WaitForAsyncOperation(motionRequest.Send()));
                        }
                    }
                }

                LoadAddressableHandle<TextAsset> poseRequest = null;
                if (!string.IsNullOrEmpty(model.pose))
                {
                    poseRequest = Load<TextAsset>($"{basePath}/{_modelName}/{model.pose}");
                    _handles.Add(poseRequest);
                    loadGroup.Add(WaitForAsyncOperation(poseRequest.Send()));
                }

                yield return loadGroup.WaitForAll();

                loaded.mocFile = mocFileRequest.Result;
                if (loaded.mocFile == null)
                {
                    Debug.LogError($"Failed to load moc file for {_modelName}");
                    OnFinishedCallback(null);
                    yield break;
                }

                for (int i = 0; i < model.textures.Length; ++i)
                {
                    loaded.textures[i] = textureRequests[i].Result;
                    if (loaded.textures[i] == null)
                    {
                        Debug.LogError($"Failed to load texture {model.textures[i]} for {_modelName}");
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
                                Debug.LogError($"Failed to load motion file {motionFiles[i].file} for {_modelName}");
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
                        Debug.LogError($"Failed to load pose file for {_modelName}");
                        OnFinishedCallback(null);
                        yield break;
                    }
                }

                OnFinishedCallback(loaded);
            }

            public override void Dispose()
            {
                foreach (var handle in _handles)
                {
                    handle.Dispose();
                }
                base.Dispose();
            }
        }
    }
}