using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Base;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class AutoLoad : Command
    {
        public AutoLoad(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -5;

        public override IEnumerator Execute()
        {
            HashSet<LoadData> allLoadData = new();

            foreach (var statement in Module.Statements)
            {
                string[] statementArgs = statement.Split(new[] { '\t' }, StringSplitOptions.None);

                // we ignore all "_load" commands since the resources they specify are sometimes not used
                // since we are doing a full scan, we can just find the actually used ones
                switch (statementArgs[0])
                {
                    case "人物":
                    case "actor_setup":
                    {
                        string argWithResource = statementArgs[1];
                        string resourceName = Module.ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.Actor, resourceName));
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
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.Sprite, resourceName));
                        break;
                    }
                    case "still":
                    {
                        string argWithResource = statementArgs[1];
                        string still = Module.ResolveAlias(argWithResource.Split(':')[0]);
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.Still, still));
                        break;
                    }
                    case "背景":
                    case "bg":
                    case "replace":
                    {
                        string bg = Module.ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.Background, bg));
                        break;
                    }
                    case "se":
                    // case "se_load":
                    case "se_loop":
                    {
                        string assetName = Module.ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.SE, assetName));
                        break;
                    }
                    case "bgm":
                    // case "bgm_load":
                    {
                        string assetName = Module.ResolveAlias(statementArgs[1]);
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.BGM, assetName));
                        break;
                    }
                    case "mes":
                    case "mes_auto":
                    {
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.Voice, statementArgs[2]));
                        break;
                    }
                    // case "voice_load":
                    // {
                    //     allLoadData.Add(new(ScenarioModuleBase.Resource.Type.Voice, statementArgs[1]));
                    //     break;
                    // }
                    case "transform_prefab":
                    {
                        string id = statementArgs[2];
                        allLoadData.Add(new(ScenarioModuleBase.Resource.Type.TransformPrefab, id));
                        break;
                    }
                }
            }

            Debug.Log($"Pre-loading {allLoadData.Count} assets...");

            // sort the hashset by load type
            var loadDataList = allLoadData.ToList();
            loadDataList.Sort((a, b) => a.Type.CompareTo(b.Type));

            CoroutineGroup group = new(loadDataList.Select(d => Module.LoadResource(d.Type, d.Name)).ToList(), Module);
            yield return group.WaitForAll();

            Debug.Log("Resource Pre-loaded");
        }

        private class LoadData : IEquatable<LoadData>
        {
            public readonly ScenarioModuleBase.Resource.Type Type;
            public readonly string Name;

            public LoadData(ScenarioModuleBase.Resource.Type type, string name)
            {
                Type = type;
                Name = name;
            }

            public bool Equals(LoadData other)
            {
                if (other == null)
                {
                    return false;
                }

                return Type == other.Type && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                if (obj is LoadData data)
                {
                    return Type == data.Type && Name == data.Name;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (int)Type * 1234 + (Name == null ? 0 : Name.GetHashCode());
            }
        }
    }
}