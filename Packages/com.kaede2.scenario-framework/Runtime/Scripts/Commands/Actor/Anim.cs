using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Anim : Command
    {
        private readonly string actorName;
        private readonly string motionName;
        private readonly bool loop;

        private Live2DActorEntity entity;

        public Anim(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            motionName = Arg(2, "");
            if (3 >= ArgLength())
                loop = true;
            else
                loop = Arg(3, "") is "LOOP_ON" or "1";
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

            entity.StartMotion(motionName, loop);
        }
    }
}