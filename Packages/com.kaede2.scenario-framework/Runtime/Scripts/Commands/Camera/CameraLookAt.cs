using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class CameraLookAt : Command
    {
        private readonly Vector2 position;
        private readonly float scale;
        private readonly float duration;
        private readonly bool wait;

        public CameraLookAt(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            position = new Vector2(Arg(1, 0.0f), Arg(2, 0.0f));
            scale = Arg(3, 1.0f);
            duration = Arg(4, 0.0f);
            wait = Arg(5, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            if (duration == 0)
            {
                Module.UIController.CameraPos = position;
                Module.UIController.CameraScale = scale;
                yield break;
            }

            Vector2 originalPosition = Module.UIController.CameraPos;
            float originalScale = Module.UIController.CameraScale;

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, position, duration,
                value => Module.UIController.CameraPos = value));
            s.Join(DOVirtual.Float(originalScale, scale, duration,
                value => Module.UIController.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}