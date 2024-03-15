﻿using System;
using System.Collections;
using biscuit.Scenario.Effect;
using Kaede2.Scenario.Entities;
using Kaede2.ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaede2.Scenario.Commands
{
    public class TransformPrefab : ScenarioModule.Command
    {
        private readonly string prefabName;
        private readonly string objName;
        private readonly CharacterId id;
        private readonly bool wait;

        private GameObject prefab;

        private GameObject Prefab
        {
            get
            {
                if (prefab != null) return prefab;
                prefab = Module.EffectPrefabs.Find(p =>
                    p.name.Equals(prefabName, StringComparison.InvariantCultureIgnoreCase));
                return prefab;
            }
        }

        public TransformPrefab(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            var split = OriginalArg(1, ":").Split(':');
            prefabName = split[0].Split('/')[^1];
            objName = split[1];
            id = (CharacterId)Arg(2, 1);
            wait = Arg(3, true);

            prefab = null;
        }

        public override ExecutionType Type => wait ? ExecutionType.Synchronous : ExecutionType.Instant;
        public override float ExpectedExecutionTime => wait ? -4 : 0;

        public override IEnumerator Execute()
        {
            if (Prefab == null)
            {
                Debug.LogError($"Animation Prefab {prefabName} not found");
                yield break;
            }

            if (!Module.ScenarioResource.transformImages.TryGetValue(id, out var transformImage))
            {
                Debug.LogError($"Transform Image for {id:G} not found");
                yield break;
            }

            GameObject instantiated = Object.Instantiate(Prefab);
            instantiated.name = objName;
            AnimationPrefabEntity entity = instantiated.AddComponent<AnimationPrefabEntity>();
            entity.prefabName = prefabName;
            CharacterTransformController controller = instantiated.GetComponent<CharacterTransformController>();
            controller.Setup(id);

            // we don't see a command with wait=true in the script
            // this behavior is guessed and could potentially result in a deadlock
            if (wait)
            {
                yield return new WaitUntil(() => entity == null);
            }
        }
    }
}