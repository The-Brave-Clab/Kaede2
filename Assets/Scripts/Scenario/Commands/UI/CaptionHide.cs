using System.Collections;
using DG.Tweening;
using Kaede2.UI.ScenarioScene;

namespace Kaede2.Scenario.Commands
{
    public class CaptionHide : ScenarioModule.Command
    {
        private readonly float duration;
        private readonly bool wait;

        private CaptionState startState;
        private CaptionBox captionBox;

        public CaptionHide(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(2, 0.0f);
            wait = Arg(3, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            captionBox = UIManager.Instance.CaptionBox;
            startState = captionBox.GetState();
            yield break;
        }

        public override IEnumerator Execute()
        {
            if (duration <= 0)
            {
                var color = captionBox.box.color;
                color.a = 0;
                captionBox.box.color = color;

                color = captionBox.text.color;
                color.a = 0;
                captionBox.text.color = color;

                captionBox.gameObject.SetActive(false);
                
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(1, 0, duration,
                value =>
                {
                    var color = captionBox.box.color;
                    color.a = value;
                    captionBox.box.color = color;

                    color = captionBox.text.color;
                    color.a = value;
                    captionBox.text.color = color;
                }));
            seq.OnComplete(() => captionBox.gameObject.SetActive(false));

            yield return seq.WaitForCompletion();
        }

        public override void Undo()
        {
            captionBox.RestoreState(startState);
        }
    }
}