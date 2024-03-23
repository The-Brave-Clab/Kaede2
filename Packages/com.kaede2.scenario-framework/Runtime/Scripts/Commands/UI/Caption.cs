using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.UI;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Caption : Command
    {
        private string resourceName;
        private string text;
        private float duration = 0.0f;
        private int fontSize = 0;
        private float x = 0.0f;
        private float y = 0.0f;
        private bool wait = true;

        private CaptionBox captionBox;

        public Caption(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            var split = Arg(1, ":").Split(':');
            resourceName = split[0]; // currently we ignore this as there's no more than one caption box at the same time
            text = split[1];
            duration = Arg(2, 0.0f);
            fontSize = Arg(3, 96);
            x = Arg(4, 0.0f);
            y = Arg(5, 0.0f);
            wait = Arg(6, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            captionBox = Module.UIController.CaptionBox;
            yield break;
        }

        public override IEnumerator Execute()
        {
            captionBox.text.text = text;
            captionBox.text.fontSize = fontSize;

            var colorStart = Module.UIController.CaptionDefaultColor;
            colorStart.a = 0;
            captionBox.box.color = colorStart;

            colorStart = captionBox.text.color;
            colorStart.a = 0;
            captionBox.text.color = colorStart;

            captionBox.gameObject.SetActive(true);

            if (duration <= 0)
            {
                captionBox.box.color = Module.UIController.CaptionDefaultColor;

                var color = captionBox.text.color;
                color.a = 1;
                captionBox.text.color = color;
                
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(0, 1, duration,
                value =>
                {
                    if (captionBox == null) return;

                    captionBox.box.color = Module.UIController.CaptionDefaultColor;

                    var color = captionBox.text.color;
                    color.a = value;
                    captionBox.text.color = color;
                }));
            yield return seq.WaitForCompletion();
        }
    }
}
