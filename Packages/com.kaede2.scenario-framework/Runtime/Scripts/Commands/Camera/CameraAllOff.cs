using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public abstract class CameraAllOnOffBase : Command
    {
        public CameraAllOnOffBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        protected abstract bool On { get; }

        public override IEnumerator Execute()
        {
            Module.UIController.CameraEnabled = On;
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