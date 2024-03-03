using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class AutoLoad : ScenarioModule.Command
    {
        public AutoLoad(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -5;

        public override IEnumerator Execute()
        {
            HashSet<LoadData> allLoadData = new();

            for (int i = 0; i < Module.StatementCount; ++i)
            {
                var statement = Module.Statement(i);
                string[] statementArgs = statement.Split(new[] { '\t' }, StringSplitOptions.None);

                switch (statementArgs[0])
                {
                    case "人物":
                    case "actor_setup":
                    {
                        string argWithResource = statementArgs[1];
                        string resourceName = Module.ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(LoadData.LoadType.Actor, resourceName));

                        break;
                    }
                    case "font":
                    {
                        break;
                    }
                    case "sprite":
                    {
                        string argWithResource = statementArgs[1];
                        string resourceName = Module.ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(LoadData.LoadType.Sprite, resourceName));
                        break;
                    }
                    case "still":
                    {
                        string argWithResource = statementArgs[1];
                        string still = Module.ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(LoadData.LoadType.Still, still));
                        break;
                    }
                    case "背景":
                    case "bg":
                    case "replace":
                    {
                        string bg = Module.ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(LoadData.LoadType.Background, bg));
                        break;
                    }
                    case "se":
                    case "se_load":
                    case "se_loop":
                    {
                        string assetName = Module.ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(LoadData.LoadType.SE, assetName));
                        break;
                    }
                    case "bgm":
                    case "bgm_load":
                    {
                        string assetName = Module.ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(LoadData.LoadType.BGM, assetName));
                        break;
                    }
                    case "mes":
                    {
                        allLoadData.Add(new(LoadData.LoadType.Voice, statementArgs[2]));
                        break;
                    }
                    case "voice_load":
                    {
                        allLoadData.Add(new(LoadData.LoadType.Voice, statementArgs[1]));
                        break;
                    }
                    case "transform_prefab":
                    {
                        string id = statementArgs[2];
                        allLoadData.Add(new(LoadData.LoadType.TransformPrefab, id));
                        break;
                    }
                }
            }

            Debug.Log($"Pre-loading {allLoadData.Count} assets...");

            // sort the hashset by load type
            var loadDataList = allLoadData.ToList();
            loadDataList.Sort((a, b) => a.loadType.CompareTo(b.loadType));

            CoroutineGroup group = new();

            IEnumerator SendHandleWithFinishCallback<T>(ResourceLoader.LoadAddressableHandle<T> handle, Action<T> callback) where T : UnityEngine.Object
            {
                yield return handle.Send();
                if (handle.Result == null)
                {
                    Debug.LogError($"Failed to load asset {handle.AssetAddress}");
                }

                callback(handle.Result);
            }

            IEnumerator SendLive2DHandleWithFinish(ResourceLoader.LoadLive2DHandle handle, string resourceName)
            {
                yield return handle.Send();
                if (handle.Result == null)
                {
                    Debug.LogError($"Failed to Live2D model {resourceName}");
                }

                Module.ScenarioResource.actors[resourceName] = handle.Result;
            }

            foreach (var data in loadDataList)
            {
                switch (data.loadType)
                {
                    case LoadData.LoadType.Sprite:
                    {
                        var handle = ResourceLoader.LoadScenarioSprite(data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, s => Module.ScenarioResource.sprites[data.resourceName] = s));
                        break;
                    }
                    case LoadData.LoadType.Still:
                    {
                        var handle = ResourceLoader.LoadScenarioStill(ScenarioModule.ScenarioName, data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, t => Module.ScenarioResource.stills[data.resourceName] = t));
                        break;
                    }
                    case LoadData.LoadType.Background:
                    {
                        var handle = ResourceLoader.LoadScenarioBackground(data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, t => Module.ScenarioResource.backgrounds[data.resourceName] = t));
                        break;
                    }
                    case LoadData.LoadType.SE:
                    {
                        var handle = ResourceLoader.LoadScenarioSoundEffect(data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, a => Module.ScenarioResource.soundEffects[data.resourceName] = a));
                        break;
                    }
                    case LoadData.LoadType.BGM:
                    {
                        var handle = ResourceLoader.LoadScenarioBackgroundMusic(data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, a => Module.ScenarioResource.backgroundMusics[data.resourceName] = a));
                        break;
                    }
                    case LoadData.LoadType.Voice:
                    {
                        var handle = ResourceLoader.LoadScenarioVoice(ScenarioModule.ScenarioName, data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, a => Module.ScenarioResource.voices[data.resourceName] = a));
                        break;
                    }
                    case LoadData.LoadType.TransformPrefab:
                    {
                        CharacterId id = (CharacterId)int.Parse(data.resourceName);
                        var handle = ResourceLoader.LoadScenarioTransformEffectSprite(id);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendHandleWithFinishCallback(handle, s => Module.ScenarioResource.transformImages[id] = s));
                        break;
                    }
                    case LoadData.LoadType.Actor:
                    {
                        var handle = ResourceLoader.LoadLive2DModel(data.resourceName);
                        Module.RegisterLoadHandle(handle);
                        group.Add(SendLive2DHandleWithFinish(handle, data.resourceName));
                        break;
                    }
                }
            }

            yield return group.WaitForAll();

            Debug.Log("Resource Pre-loaded");
        }

        private class LoadData : IEquatable<LoadData>
        {
            // the order of these enum values is important
            // it's used to hint the general size of the asset to be loaded
            // so larger assets are loaded first to save time
            public enum LoadType
            {
                BGM,
                Voice,
                SE,
                Actor,
                Background,
                Still,
                Sprite,
                TransformPrefab,
            }

            public readonly LoadType loadType;
            public readonly string resourceName;

            public LoadData(LoadType type, string name)
            {
                loadType = type;
                resourceName = name;
            }

            public bool Equals(LoadData other)
            {
                if (other == null)
                {
                    return false;
                }

                return loadType == other.loadType && resourceName == other.resourceName;
            }

            public override bool Equals(object obj)
            {
                if (obj is LoadData data)
                {
                    return loadType == data.loadType && resourceName == data.resourceName;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (int)loadType * 1234 + (resourceName == null ? 0 : resourceName.GetHashCode());
            }
        }
    }
}