using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class MoveAnim : Command
    {
        private readonly string entityName;
        private readonly Vector3 posDiff;
        private readonly float duration;
        private readonly bool rebound;
        private readonly int loop;
        private readonly Ease ease;
        private readonly bool wait;

        private Entity entity;

        public MoveAnim(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            posDiff = new Vector3(Arg(2, 0.0f), Arg(3, 0.0f), 0);
            duration = Arg(4, 0.0f);
            rebound = Arg(5, true);
            loop = Arg(6, 0);
            ease = Arg(7, Ease.Linear);
            wait = Arg(8, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, ExpectedExecutionTime);
        public override float ExpectedExecutionTime => duration * (loop < 0 ? -1 : loop + 1) * (rebound ? 2 : 1);

        public override IEnumerator Setup()
        {
            FindEntity(entityName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            var originalPos = entity.Position;
            Vector3 targetPos = originalPos + posDiff;

            if (ExpectedExecutionTime == 0)
            {
                if (!rebound)
                    entity.Position = targetPos;
                yield break;
            }

            yield return entity.MoveAnim(originalPos, targetPos, duration, rebound, loop, ease);
        }
    }
}