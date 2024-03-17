using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class End : ScenarioModule.Command
    {
        public End(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.Initialized = false;
            Debug.Log("Scenario ended");
            yield break;
        }
    }
}