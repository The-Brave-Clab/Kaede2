using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorAngle : ScenarioModule.Command
    {
        private readonly string actorName;
        private readonly float angleX;
        private readonly float angleY;
        private readonly float duration;
        private readonly bool wait;

        private Live2DActorEntity entity;

        public ActorAngle(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            angleX = Arg(2, 0.0f);
            angleY = Arg(3, 0.0f);
            duration = Arg(4, 0.0f);
            wait = Arg(5, false);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            FindEntity(actorName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (entity != null)
                yield return entity.ActorAngle(angleX, angleY, duration);
            else
                Debug.LogError($"Live2D Actor Entity {actorName} not found");
        }
    }
}