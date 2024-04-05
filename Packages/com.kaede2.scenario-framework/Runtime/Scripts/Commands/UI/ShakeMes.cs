using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.UI;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class ShakeMes : Command
    {
        private readonly bool wait;
        private readonly float duration;
        private readonly float strength;
        private readonly int vibrato;

        private MessageBox messageBox;

        public ShakeMes(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 0f);
            strength = Arg(2, 20f);
            vibrato = Arg(3, 10);
            wait = Arg(4, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override void Setup()
        {
            messageBox = Module.UIController.MessageBox;
        }

        public override IEnumerator Execute()
        {
            if (duration == 0)
            {
                yield break;
            }

            Vector2 originalPos = messageBox.Position;

            Sequence s = DOTween.Sequence();
            s.Append(DOTween.Punch(
                () => messageBox.Position,
                value =>
                {
                    Vector3 pos = messageBox.Position;
                    pos.x = -value.x;
                    pos.y = -value.y;
                    messageBox.Position = pos;
                },
                Vector3.one * strength,
                duration,
                vibrato,
                1.0f));

            yield return s.WaitForCompletion();

            messageBox.Position = originalPos;
        }
    }
}