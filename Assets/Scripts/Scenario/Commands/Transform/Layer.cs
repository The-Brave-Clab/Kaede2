﻿using System.Collections;
using Kaede2.Scenario.Entities;

namespace Kaede2.Scenario.Commands
{
    public class Layer : ScenarioModule.Command
    {
        private readonly string entityName;
        private readonly int layer;

        private Live2DActorEntity entity;

        public Layer(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            layer = Arg(2, 0);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(entityName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            entity.Layer = layer;
            yield break;
        }
    }
}