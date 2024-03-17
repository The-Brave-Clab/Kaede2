using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorExit : Command
    {
        private readonly string actorName;
        private readonly float duration;
        private readonly string direction;
        private readonly bool wait;

        private Live2DActorEntity entity;

        public ActorExit(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            duration = Arg(2, 0.0f);
            direction = Arg(3, "右");
            wait = Arg(4, true);
        }

        public override ExecutionType Type { get; }
        public override float ExpectedExecutionTime { get; }

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

            if (duration == 0)
            {
                yield break;
            }

            Vector3 pos = entity.Position;
            Vector3 targetPos = pos;
            float num = 1920.0f / 2.0f + 1920.0f / 3.0f;
            targetPos.x = direction == "右" ? num : -num;

            yield return entity.ActorExit(pos, targetPos, duration);
        }
    }
}