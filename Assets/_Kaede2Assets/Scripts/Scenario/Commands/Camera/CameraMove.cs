using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Base;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class CameraMove : Command
    {
        private readonly Vector2 position;
        private readonly float duration;
        private readonly bool wait;

        public CameraMove(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            if (duration == 0)
            {
                Module.UIController.CameraPos = position;
                yield break;
            }

            Vector2 originalPosition = Module.UIController.CameraPos;

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, position, duration,
                value => Module.UIController.CameraPos = value));

            yield return s.WaitForCompletion();
        }
    }
}