using System.Collections;
using DG.Tweening;
using Kaede2.UI.ScenarioScene;

namespace Kaede2.Scenario.Commands
{
    public abstract class FadeBase : ScenarioModule.Command
    {
        private readonly float duration;
        private readonly bool wait;

        protected abstract float From { get; }
        protected abstract float To { get; }

        private FadeState startState;

        protected FadeBase(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 1.0f);
            wait = Arg(2, true);
        }

        public override ExecutionType Type
        {
            get
            {
                if (duration == 0) return ExecutionType.Instant;
                if (wait) return ExecutionType.Synchronous;
                return ExecutionType.Asynchronous;
            }
        }

        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            startState = UIManager.Instance.fade.GetState();
            yield break;
        }

        public override IEnumerator Execute()
        {
            var fade = UIManager.Instance.fade;
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

        public override void Undo()
        {
            UIManager.Instance.fade.RestoreState(startState);
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