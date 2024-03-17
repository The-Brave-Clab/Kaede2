using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Shake : Command
    {
        private readonly float duration;
        private readonly float strength;
        private readonly int vibrato;
        private readonly bool wait;

        public Shake(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 0.0f);
            strength = Arg(2, 20.0f);
            vibrato = Arg(3, 10);
            wait = Arg(4, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;
        public override IEnumerator Execute()
        {
            if (duration == 0)
            {
                yield break;
            }

            Vector2 originalPos = Module.UIManager.CameraPos;

            Sequence s = DOTween.Sequence();
            s.Append(DOTween.Punch(
                () => Module.UIManager.CameraPos,
                value =>
                {
                    Vector3 pos = Module.UIManager.CameraPos;
                    pos.x = -value.x; // * canvasScale;
                    Module.UIManager.CameraPos = pos;
                },
                Vector3.one * strength,
                duration,
                vibrato,
                1.0f));

            yield return s.WaitForCompletion();

            Module.UIManager.CameraPos = originalPos;
        }
    }
}