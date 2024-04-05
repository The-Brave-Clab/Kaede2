using System.Collections;
using DG.Tweening;
using Kaede2.Scenario.Framework.UI;

namespace Kaede2.Scenario.Framework.Commands
{
    public abstract class FadeBase : Command
    {
        private readonly float duration;
        private readonly bool wait;

        protected abstract float From { get; }
        protected abstract float To { get; }

        private FadeState startState;
        private FadeTransition fade;

        protected FadeBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 1.0f);
            wait = Arg(2, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override void Setup()
        {
            fade = Module.UIController.Fade;
            startState = Module.UIController.Fade.GetState();
        }

        public override IEnumerator Execute()
        {
            fade.progress = From;

            if (duration <= 0)
            {
                fade.progress = To;
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(From, To, duration, value => { fade.progress = value; }));
            yield return seq.WaitForCompletion();
        }
    }

    public class FadeIn : FadeBase
    {
        public FadeIn(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        protected override float From => 0;
        protected override float To => 1;
    }
}