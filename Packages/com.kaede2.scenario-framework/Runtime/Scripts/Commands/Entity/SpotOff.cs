﻿using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class SpotOff : Command
    {
        private Entity[] allEntities = null;

        public SpotOff(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            allEntities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            yield break;
        }

        public override IEnumerator Execute()
        {
            foreach (var entity in allEntities)
            {
                var alpha = entity.GetColor().a;
                entity.SetColor(new UnityEngine.Color(1.0f, 1.0f, 1.0f, alpha));
            }
            yield break;
        }
    }
}