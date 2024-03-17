using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class AnimationPrefabHide : ScenarioModule.Command
    {
        private readonly string entityName;

        private AnimationPrefabEntity entity;

        public AnimationPrefabHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(entityName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Animation Prefab Entity {entityName} not found");
                yield break;
            }

            Object.Destroy(entity.gameObject);
        }
    }
}