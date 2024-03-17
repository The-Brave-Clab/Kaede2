using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class CameraDefault : ScenarioModule.Command
    {
        private readonly float duration;
        private readonly bool wait;

        public CameraDefault(ScenarioModule module, string[] arguments) : base(module, arguments)
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
                UIManager.CameraPos = UIManager.CameraPosDefault;
                UIManager.CameraScale = UIManager.CameraScaleDefault;
                yield break;
            }

            var originalPosition = UIManager.CameraPos;
            var originalScale = UIManager.CameraScale;

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, UIManager.CameraPosDefault, duration,
                value => UIManager.CameraPos = value));
            s.Join(DOVirtual.Float(originalScale, UIManager.CameraScaleDefault, duration,
                value => UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}