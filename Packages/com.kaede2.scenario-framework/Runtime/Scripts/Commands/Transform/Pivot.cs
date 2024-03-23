using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Pivot : Command
    {
        private readonly string entityName;
        private readonly Vector2 pivot;

        private Entity entity;

        public Pivot(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            entityName = OriginalArg(1);
            pivot = new Vector2(Arg(2, 0.0f), Arg(3, 0.0f));
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
            entity.Pivot = pivot;
            yield break;
        }
    }
}