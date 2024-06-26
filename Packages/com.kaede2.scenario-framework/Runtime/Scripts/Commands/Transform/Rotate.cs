﻿using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Rotate : Command
    {
        private readonly string entityName;
        private readonly float angle;
        private readonly float duration;
        private readonly Ease ease;
        private readonly bool wait;

        private Entity entity;
        public Rotate(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            angle = Arg(2, 0.0f);
            duration = Arg(3, 0.0f);
            ease = Arg(4, Ease.Linear);
            wait = Arg(5, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, ExpectedExecutionTime);
        public override float ExpectedExecutionTime => duration;

        public override void Setup()
        {
            FindEntity(entityName, out entity);
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Entity {entityName} not found");
                yield break;
            }

            var euler = entity.transform.eulerAngles;
            var originalAngle = euler.z;
            float targetAngle = angle;

            if (duration == 0)
            {
                euler.z = targetAngle;
                entity.transform.eulerAngles = euler;
                yield break;
            }

            entity.EnsureRotateStopped();
            yield return entity.Rotate(originalAngle, targetAngle, duration, ease);
        }
    }
}