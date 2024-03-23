using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class StillOff : Command
    {
        private readonly string objName;

        private BackgroundEntity entity;

        public StillOff(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            objName = OriginalArg(1);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            FindEntity(objName, out entity);
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (entity == null)
            {
                Debug.LogError($"Still Entity {objName} not found");
                yield break;
            }

            Object.Destroy(entity.gameObject);
        }
    }
}