using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class ActorEyeOff : Command
    {
        private readonly string actorName;

        private Live2DActorEntity entity;

        public ActorEyeOff(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            FindEntity(actorName, out entity);
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Live2D Actor Entity {actorName} not found");
                yield break;
            }

            entity.AddEyeX = 0;
            entity.AbsoluteEyeX = 0;
        }
    }
}