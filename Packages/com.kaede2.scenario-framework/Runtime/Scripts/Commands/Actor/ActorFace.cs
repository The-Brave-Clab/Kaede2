﻿using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class ActorFace : Command
    {
        private readonly string actorName;
        private readonly string faceMotionName;

        private Live2DActorEntity entity;

        public ActorFace(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            actorName = OriginalArg(1);
            faceMotionName = Arg(2, "");
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

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

            entity.StartFaceMotion(faceMotionName);
        }
    }
}