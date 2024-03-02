using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Del : ScenarioModule.Command
    {
        private readonly string objectName;

        private ScenarioModule.Entity entity;

        public Del(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            objectName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            if (FindEntity(objectName, out entity) == 0) yield break;
            Debug.LogError($"Entity {objectName} not found"); // for del command we only do precise match
            entity = null;
        }

        public override IEnumerator Execute()
        {
            Object.Destroy(entity.gameObject);
            yield break;
        }
    }
}