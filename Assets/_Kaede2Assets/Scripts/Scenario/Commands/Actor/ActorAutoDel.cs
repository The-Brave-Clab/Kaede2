using System.Collections;

namespace Kaede2.Scenario.Commands
{
    public class ActorAutoDel : Command
    {
        private readonly bool value;

        public ActorAutoDel(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            value = Arg(1, false);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.ActorAutoDelete = value;
            yield break;
        }
    }
}