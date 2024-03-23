namespace Kaede2.Scenario.Framework.Commands
{
    public class CameraAllOn : CameraAllOnOffBase
    {
        public CameraAllOn(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool On => true;
    }
}