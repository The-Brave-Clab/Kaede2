namespace Kaede2.Scenario.Commands
{
    public class MesAuto : Mes
    {
        public MesAuto(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Asynchronous;
    }
}