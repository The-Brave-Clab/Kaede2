using System.Collections;
using Kaede2.Scenario.Framework.Entities;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Layer : Command
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

        public override void Setup()
        {
            FindEntity(entityName, out entity);
        }

        public override IEnumerator Execute()
        {
            entity.Layer = layer;
            yield break;
        }
    }
}