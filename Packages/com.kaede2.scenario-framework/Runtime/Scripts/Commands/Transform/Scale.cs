using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Scale : Command
    {
        private readonly string entityName;
        private readonly float scale;
        private readonly float duration;
        private readonly Ease ease;
        private readonly bool wait;

        private Entity entity;
        public Scale(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            scale = Arg(2, 0.0f);
            duration = Arg(3, 0.0f);
            ease = Arg(4, Ease.Linear);
            wait = Arg(5, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, ExpectedExecutionTime);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            FindEntity(entityName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Entity {entityName} not found");
                yield break;
            }

            var originalScale = entity.transform.localScale;
            var targetScale = scale * Vector3.one;

            if (duration == 0)
            {
                entity.transform.localScale = targetScale;
                yield break;
            }

            yield return entity.Scale(originalScale, targetScale, duration, ease);
        }
    }
}