using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorScale : ScenarioModule.Command
    {
        private readonly string actorName;
        private readonly float scale;
        private readonly float duration;
        private readonly bool wait;

        private Live2DActorEntity entity;

        public ActorScale(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            scale = Arg(2, 1.0f);
            duration = Arg(3, 0.0f);
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

            RectTransform rt = entity.GetComponent<RectTransform>();
            if (duration == 0)
            {
                rt.localScale = Vector3.one * scale;
                yield break;
            }

            yield return entity.ActorScale(scale, duration);
        }
    }
}