using System.Collections;
using Kaede2.Scenario.Audio;

namespace Kaede2.Scenario.Commands
{
    public class BGMStop : ScenarioModule.Command
    {
        private readonly float duration;

        private AudioState state;

        public BGMStop(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 0.0f);
        }

        public override ExecutionType Type => duration <= 0 ? ExecutionType.Instant : ExecutionType.Asynchronous;
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            state = AudioManager.Instance.GetState();
            yield break;
        }

        public override IEnumerator Execute()
        {
            var task = AudioManager.Instance.StopBGM(duration);
            if (task != null)
                yield return task;
        }

        public override void Undo()
        {
            AudioManager.Instance.RestoreState(state);
        }
    }
}