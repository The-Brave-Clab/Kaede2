using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorEnter : ScenarioModule.Command
    {
        private readonly string actorName;
        private readonly float duration;
        private readonly string direction;
        private readonly bool wait;

        private Live2DActorEntity entity;

        public ActorEnter(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            duration = Arg(2, 0.0f);
            direction = Arg(3, "右");
            wait = Arg(4, true);
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
            if (entity == null)
            {
                Debug.LogError($"Live2D Actor Entity {actorName} not found");
                yield break;
            }

            Vector3 pos = entity.Position;
            Vector3 targetPos = pos;
            float num = 1920.0f / 2.0f + 1920.0f / 6.0f;
            targetPos.x = direction == "右" ? num : -num;

            yield return entity.ActorEnter(pos, targetPos, duration);
        }
    }
}