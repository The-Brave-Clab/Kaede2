using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public class InitEnd : Command
    {
        public InitEnd(ScenarioModule module, string[] arguments) : base(module, arguments)
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