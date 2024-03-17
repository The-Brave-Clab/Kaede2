using System.Collections;
using Kaede2.Scenario.Base;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Del : Command
    {
        private readonly string objectName;

        private Entity entity;

        public Del(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            objectName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            var entities = Object.FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (entities == null || entities.Length == 0)
            {
                entity = null;
                yield break;
            }

            foreach (var e in entities)
            {
                if (e.name == objectName)
                {
                    entity = e;
                    yield break;
                }
            }

            entity = null;
        }

        public override IEnumerator Execute()
        {
            Object.Destroy(entity.gameObject);
            yield break;
        }
    }
}