using System;
using System.Collections;
using biscuit.Scenario.Effect;
using UnityEngine;
using Kaede2.Scenario.Entities;
using Kaede2.ScriptableObjects;
using Object = UnityEngine.Object;

namespace Kaede2.Scenario.Commands
{
    public class AnimationPrefab : ScenarioModule.Command
    {
        private readonly string prefabName;
        private readonly string objectName;
        private readonly Vector2 position;
        private readonly float scale;
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

        public AnimationPrefab(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            var split = OriginalArg(1, ":").Split(":");
            prefabName = split[0].Split('/')[^1];
            objectName = split[1];
            position = new Vector2(Arg(2, 0.0f), Arg(3, 0.0f));
            scale = Arg(4, 1.0f);
            wait = Arg(5, true);

            prefab = null;
        }

        public override ExecutionType Type => wait ? ExecutionType.Synchronous : ExecutionType.Instant;

        public override float ExpectedExecutionTime
        {
            get
            {
                if (!wait) return 0;

                if (Prefab == null) return 0;

                if (Prefab.TryGetComponent<EffectDestroyer>(out var destroyer))
                {
                    return -destroyer.DeathTimer;
                }

                if (Prefab.TryGetComponent<TimelineCallbacker>(out _))
                {
                    return -2.167f; // check Adv_Screen_cutscene_001.anim
                }

                if (Prefab.TryGetComponent<CharacterTransformController>(out _))
                {
                    return -4; // check transform_prefab usage in scripts
                }

                return -5; // persistent effect
            }
        }

        public override IEnumerator Execute()
        {
            if (Prefab == null)
            {
                Debug.LogError($"Animation Prefab {prefabName} not found");
                yield break;
            }

            GameObject instantiated = Object.Instantiate(Prefab);
            instantiated.name = objectName;
            Transform transform = instantiated.transform;
            transform.localScale = Vector3.one * scale;
            AnimationPrefabEntity entity = instantiated.AddComponent<AnimationPrefabEntity>();
            entity.prefabName = prefabName;
            entity.Position = position;

            // bg_effect_prefab command always has wait=true while animation_prefab always has wait=false
            // it seems that this parameter actually affects nothing
            // we at least use it to define the execution type
        }
    }
}