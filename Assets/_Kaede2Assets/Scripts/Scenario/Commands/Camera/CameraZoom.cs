using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.UI;

namespace Kaede2.Scenario.Commands
{
    public class CameraZoom : ScenarioModule.Command
    {
        private readonly float scale;
        private readonly float duration;
        private readonly bool wait;

        public CameraZoom(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            scale = Arg(1, 1.0f);
            duration = Arg(2, 0.0f);
            wait = Arg(3, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            if (duration == 0)
            {
                UIManager.CameraScale = scale;
                yield break;
            }

            float originalScale = UIManager.CameraScale;
            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Float(originalScale, scale, duration,
                value => UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}