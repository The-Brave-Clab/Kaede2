using Kaede2.Scenario.Base;

namespace Kaede2.Scenario.Commands
{
    public class RotateAnimStop : AnimStopBase
    {
        public RotateAnimStop(ScenarioModuleBase module, string[] arguments) : base(module, arguments) { }

        protected override string AnimName => "rotate";
    }
}