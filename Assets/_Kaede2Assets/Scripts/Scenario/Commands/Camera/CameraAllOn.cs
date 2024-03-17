namespace Kaede2.Scenario.Commands
{
    public class CameraAllOn : CameraAllOnOffBase
    {
        public CameraAllOn(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool On => true;
    }
}