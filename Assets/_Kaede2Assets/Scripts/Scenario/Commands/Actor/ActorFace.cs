using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorFace : Command
    {
        private readonly string actorName;
        private readonly string faceMotionName;

        private Live2DActorEntity entity;

        public ActorFace(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            faceMotionName = Arg(2, "");
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

            entity.StartFaceMotion(faceMotionName);
        }
    }
}