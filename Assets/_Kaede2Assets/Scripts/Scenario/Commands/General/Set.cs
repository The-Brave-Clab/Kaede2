using System.Collections;
using Kaede2.Scenario.Base;

namespace Kaede2.Scenario.Commands
{
    public class Set : Command
    {
        private readonly string variable;
        private readonly string value;

        public Set(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            string[] split = OriginalArg(1).Split('=');
            variable = split[0].Trim();
            value = split[1].Trim();
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.AddVariable(variable, value);
            yield break;
        }
    }
}