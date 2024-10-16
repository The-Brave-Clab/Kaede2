﻿using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class SpriteHide : Command
    {
        private readonly string entityName;
        private readonly float duration;
        private readonly bool wait;

        private SpriteEntity entity;

        public SpriteHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            duration = Arg(2, 0.0f);
            wait = Arg(3, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override void Setup()
        {
            FindEntity(entityName, out entity);
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
                entity.Destroy();
                yield break;
            }

            yield return entity.ColorAlpha(color, color.a, 0, duration, true);
        }
    }
}