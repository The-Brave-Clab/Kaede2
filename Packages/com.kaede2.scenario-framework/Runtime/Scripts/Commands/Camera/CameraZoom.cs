using System.Collections;
using DG.Tweening;

namespace Kaede2.Scenario.Framework.Commands
{
    public class CameraZoom : Command
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
                Module.UIController.CameraScale = scale;
                yield break;
            }

            float originalScale = Module.UIController.CameraScale;
            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Float(originalScale, scale, duration,
                value => Module.UIController.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}