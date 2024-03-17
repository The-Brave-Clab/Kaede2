using System.Collections;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public abstract class CameraAllOnOffBase : ScenarioModule.Command
    {
        public CameraAllOnOffBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        protected abstract bool On { get; }

        public override IEnumerator Execute()
        {
            UIManager.Instance.contentCanvas.gameObject.SetActive(On);
            yield break;
        }
    }

    public class CameraAllOff : CameraAllOnOffBase
    {
        public CameraAllOff(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override bool On => false;
    }
}