using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class CameraLookAt : Command
    {
        private readonly Vector2 position;
        private readonly float scale;
        private readonly float duration;
        private readonly bool wait;

        public CameraLookAt(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
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
                Module.UIManager.CameraPos = position;
                Module.UIManager.CameraScale = scale;
                yield break;
            }

            Vector2 originalPosition = Module.UIManager.CameraPos;
            float originalScale = Module.UIManager.CameraScale;

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, position, duration,
                value => Module.UIManager.CameraPos = value));
            s.Join(DOVirtual.Float(originalScale, scale, duration,
                value => Module.UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}