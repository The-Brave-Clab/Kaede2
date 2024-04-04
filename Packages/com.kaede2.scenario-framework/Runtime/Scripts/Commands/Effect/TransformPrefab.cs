using System;
using System.Collections;
using System.Linq;
using biscuit.Scenario.Effect;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaede2.Scenario.Framework.Commands
{
    public class TransformPrefab : Command
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
                prefab = Module.EffectPrefabs.FirstOrDefault(p =>
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

            if (!Module.ScenarioResource.TransformImages.TryGetValue(id, out var transformImage))
            {
                Debug.LogError($"Transform Image for {id:G} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
                yield break;
            }

            GameObject instantiated = Object.Instantiate(Prefab);
            instantiated.name = objName;
            AnimationPrefabEntity entity = instantiated.AddComponent<AnimationPrefabEntity>();
            entity.Module = Module;
            entity.prefabName = prefabName;
            CharacterTransformController controller = instantiated.GetComponent<CharacterTransformController>();
            controller.Module = Module;
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