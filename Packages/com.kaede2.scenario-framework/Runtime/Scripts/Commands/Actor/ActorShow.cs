namespace Kaede2.Scenario.Framework.Commands
{
    public class ActorShow : ActorShowHideBase
    {
        public ActorShow(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Hide => false;
    }
}