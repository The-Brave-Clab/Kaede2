using System.Collections;
using Kaede2.Scenario.Base;

namespace Kaede2.Scenario.Commands
{
    public class InitEnd : Command
    {
        public InitEnd(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.InitEnd();
            yield break;
        }
    }
}