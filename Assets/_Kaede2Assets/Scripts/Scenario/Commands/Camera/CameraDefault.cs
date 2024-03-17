using System.Collections;
using DG.Tweening;
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
                Module.UIManager.CameraPos = UIManager.CameraPosDefault;
                Module.UIManager.CameraScale = UIManager.CameraScaleDefault;
                yield break;
            }

            var originalPosition = Module.UIManager.CameraPos;
            var originalScale = Module.UIManager.CameraScale;

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, UIManager.CameraPosDefault, duration,
                value => Module.UIManager.CameraPos = value));
            s.Join(DOVirtual.Float(originalScale, UIManager.CameraScaleDefault, duration,
                value => Module.UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}