using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public abstract class AnimStopBase : Command
    {
        private readonly string entityName;

        private Entity entity;

        protected abstract string AnimName { get; }

        protected AnimStopBase(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
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
                Debug.LogError($"Entity {entityName} not found");
                yield break;
            }

            entity.StopAnim(AnimName);
        }
    }

    public class MoveAnimStop : AnimStopBase
    {
        public MoveAnimStop(ScenarioModuleBase module, string[] arguments) : base(module, arguments) { }

        protected override string AnimName => "move";
    }
}