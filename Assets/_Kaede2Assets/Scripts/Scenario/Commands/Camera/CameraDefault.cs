using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class CameraDefault : Command
    {
        private readonly float duration;
        private readonly bool wait;

        public CameraDefault(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 0.0f);
            wait = Arg(2, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            if (duration == 0)
            {
                Module.UIController.CameraPos = UIController.CameraPosDefault;
                Module.UIController.CameraScale = UIController.CameraScaleDefault;
                yield break;
            }

            var originalPosition = Module.UIController.CameraPos;
            var originalScale = Module.UIController.CameraScale;

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, UIController.CameraPosDefault, duration,
                value => Module.UIController.CameraPos = value));
            s.Join(DOVirtual.Float(originalScale, UIController.CameraScaleDefault, duration,
                value => Module.UIController.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}