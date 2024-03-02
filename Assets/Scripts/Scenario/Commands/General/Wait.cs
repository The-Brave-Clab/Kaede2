using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Wait : ScenarioModule.Command
    {
        private readonly float duration;

        public Wait(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 0.0f);
        }

        public override ExecutionType Type => duration == 0 ? ExecutionType.Instant : ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            if (duration == 0) yield break;
            yield return new WaitForSeconds(duration);
        }
    }
}