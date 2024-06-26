﻿using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class ActorEyeAdd : Command
    {
        private readonly string actorName;
        private readonly float value;
        private readonly float duration;
        private readonly bool wait;

        private Live2DActorEntity entity;

        public ActorEyeAdd(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            value = Arg(2, 0.0f);
            duration = Arg(3, 0.0f);
            wait = Arg(4, false);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override void Setup()
        {
            FindEntity(actorName, out entity);
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
                entity.AddEyeX = value;
                entity.AbsoluteEyeX = 0;
                yield break;
            }

            yield return entity.ActorEyeAdd(value, duration);
        }
    }
}