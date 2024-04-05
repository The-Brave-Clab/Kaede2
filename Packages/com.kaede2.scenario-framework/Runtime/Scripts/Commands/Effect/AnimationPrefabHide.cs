using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class AnimationPrefabHide : Command
    {
        private readonly string entityName;

        private AnimationPrefabEntity entity;

        public AnimationPrefabHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            FindEntity(entityName, out entity);
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Animation Prefab Entity {entityName} not found");
                yield break;
            }

            entity.Destroy();
        }
    }
}