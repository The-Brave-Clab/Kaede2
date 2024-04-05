using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Del : Command
    {
        private readonly string objectName;

        private Entity entity;

        public Del(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            objectName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            FindEntity(objectName, out entity);
        }

        public override IEnumerator Execute()
        {
            entity.Destroy();
            yield break;
        }
    }
}