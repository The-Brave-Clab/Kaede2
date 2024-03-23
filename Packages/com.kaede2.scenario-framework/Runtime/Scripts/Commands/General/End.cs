using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public class End : Command
    {
        public End(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.End();
            yield break;
        }
    }
}