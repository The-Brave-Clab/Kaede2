namespace Kaede2.Scenario.Commands
{
    public class FadeOut : FadeBase
    {
        public FadeOut(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override float From => 1;
        protected override float To => 0;
    }
}