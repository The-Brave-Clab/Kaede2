namespace Kaede2.Scenario.Commands
{
    public class RotateAnimStop : AnimStopBase
    {
        public RotateAnimStop(ScenarioModule module, string[] arguments) : base(module, arguments) { }

        protected override string AnimName => "rotate";
    }
}