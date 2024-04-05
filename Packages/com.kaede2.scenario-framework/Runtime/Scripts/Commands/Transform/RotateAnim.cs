using System.Collections;
using DG.Tweening;

namespace Kaede2.Scenario.Framework.Commands
{
    public class RotateAnim : Command
    {
        private readonly string entityName;
        private readonly float angleDiff;
        private readonly float duration;
        private readonly bool rebound;
        private readonly int loop;
        private readonly Ease ease;
        private readonly bool wait;

        private Entity entity;

        public RotateAnim(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            angleDiff = Arg(2, 0.0f);
            duration = Arg(3, 0.0f);
            rebound = Arg(4, true);
            loop = Arg(5, 0);
            ease = Arg(6, Ease.Linear);
            wait = Arg(7, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, ExpectedExecutionTime);
        public override float ExpectedExecutionTime => duration * (loop < 0 ? -1 : loop + 1) * (rebound ? 2 : 1);

        public override void Setup()
        {
            FindEntity(entityName, out entity);
        }

        public override IEnumerator Execute()
        {
            var euler = entity.transform.eulerAngles;
            var originalAngle = euler.z;
            float targetAngle = originalAngle + angleDiff;

            if (ExpectedExecutionTime == 0)
            {
                if (!rebound)
                    entity.transform.eulerAngles = new UnityEngine.Vector3(euler.x, euler.y, targetAngle);
                yield break;
            }

            yield return entity.RotateAnim(originalAngle, targetAngle, duration, rebound, loop, ease);
        }
    }
}