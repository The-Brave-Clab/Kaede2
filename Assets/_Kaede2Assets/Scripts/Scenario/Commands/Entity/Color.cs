using System.Collections;
using DG.Tweening;

namespace Kaede2.Scenario.Commands
{
    public class Color : Command
    {
        private readonly string entityName;
        private readonly UnityEngine.Color color;
        private readonly float duration;
        private readonly Ease ease;
        private readonly bool wait;

        private Entity entity;

        public Color(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            color = new UnityEngine.Color(Arg(2, 0.0f), Arg(3, 0.0f), Arg(4, 0.0f), Arg(5, 1.0f));
            duration = Arg(6, 0.0f);
            ease = Arg(7, Ease.Linear);
            wait = Arg(8, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            FindEntity(entityName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            var originalColor = entity.GetColor();

            if (duration == 0)
            {
                entity.SetColor(color);
                yield break;
            }

            yield return entity.ColorCommand(originalColor, color, duration, ease);
        }
    }
}