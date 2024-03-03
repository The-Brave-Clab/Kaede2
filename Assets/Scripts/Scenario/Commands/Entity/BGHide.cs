using System.Collections;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class BGHide : ScenarioModule.Command
    {
        private readonly string objName;

        private BackgroundEntity entity;

        public BGHide(ScenarioModule module, string[] arguments) : base(module, arguments)
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
                Debug.LogError($"Background Entity {objName} not found");
                yield break;
            }

            entity.gameObject.SetActive(false);
        }
    }
}