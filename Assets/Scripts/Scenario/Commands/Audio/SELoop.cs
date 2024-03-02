namespace Kaede2.Scenario.Commands
{
    public class SELoop : SE
    {
        public SELoop(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool Loop => true;
    }
}