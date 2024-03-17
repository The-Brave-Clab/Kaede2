using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class SpriteHide : Command
    {
        private readonly string entityName;
        private readonly float duration;
        private readonly bool wait;

        private SpriteEntity entity;

        public SpriteHide(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            duration = Arg(2, 0.0f);
            wait = Arg(3, true);
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
            if (entity == null)
            {
                Debug.LogError($"Sprite {entityName} not found");
                yield break;
            }

            var color = entity.GetColor();

            if (duration == 0)
            {
                entity.SetColor(new(color.r, color.g, color.b, 0));
                Object.Destroy(entity.gameObject);
                yield break;
            }

            yield return entity.ColorAlpha(color, color.a, 0, duration, true);
        }
    }
}