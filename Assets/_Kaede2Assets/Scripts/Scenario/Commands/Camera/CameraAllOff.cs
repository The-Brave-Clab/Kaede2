using System.Collections;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public abstract class CameraAllOnOffBase : Command
    {
        public CameraAllOnOffBase(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        protected abstract bool On { get; }

        public override IEnumerator Execute()
        {
            Module.UIManager.contentCanvas.gameObject.SetActive(On);
            yield break;
        }
    }

    public class CameraAllOff : CameraAllOnOffBase
    {
        public CameraAllOff(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool On => false;
    }
}