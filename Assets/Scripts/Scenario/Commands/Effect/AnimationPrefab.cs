using System;
using System.Collections;
using UnityEngine;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using Object = UnityEngine.Object;

namespace Kaede2.Scenario.Commands
{
    public class AnimationPrefab : ScenarioModule.Command
    {
        private readonly string prefabName;
        private readonly string objectName;
        private readonly Vector2 position;
        private readonly float scale;
        private bool wait;

        public AnimationPrefab(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            var split = OriginalArg(1, ":").Split(":");
            prefabName = split[0].Split('/')[^1];
            objectName = split[1];
            position = new Vector2(Arg(2, 0.0f), Arg(3, 0.0f));
            scale = Arg(4, 1.0f);
            wait = Arg(5, true);
        }

        public override ExecutionType Type => wait ? ExecutionType.Synchronous : ExecutionType.Instant;
        public override float ExpectedExecutionTime => wait ? -1 : 0;
        public override IEnumerator Execute()
        {
            GameObject prefab = Module.EffectPrefabs.Find(p =>
                p.name.Equals(prefabName, StringComparison.InvariantCultureIgnoreCase));

            if (prefab == null)
            {
                Debug.LogError($"Animation Prefab {prefabName} not found");
                yield break;
            }

            GameObject instantiated = Object.Instantiate(prefab);
            instantiated.name = objectName;
            Transform transform = instantiated.transform;
            transform.localScale = Vector3.one * scale;
            AnimationPrefabEntity entity = instantiated.AddComponent<AnimationPrefabEntity>();
            entity.Position = position;

            // we don't see a command with wait=true in the script
            // this behavior is guessed and could potentially result in a deadlock
            if (wait)
            {
                yield return new WaitUntil(() => entity == null);
            }
        }
    }
}