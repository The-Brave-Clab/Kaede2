namespace Kaede2.Scenario.Commands
{
    public class ActorShow : ActorShowHideBase
    {
        public ActorShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Hide => false;
    }
}