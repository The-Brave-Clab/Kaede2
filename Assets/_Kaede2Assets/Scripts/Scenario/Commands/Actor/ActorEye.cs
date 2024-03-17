using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorEye : Command
    {
        private readonly string actorName;
        private readonly string motion;

        private Live2DActorEntity entity;

        public ActorEye(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            motion = Arg(2, "");
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(actorName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Live2D Actor Entity {actorName} not found");
                yield break;
            }

            entity.SetEye(motion);
        }
    }
}