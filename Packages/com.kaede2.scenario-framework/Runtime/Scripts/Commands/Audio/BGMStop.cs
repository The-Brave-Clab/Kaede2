using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public class BGMStop : Command
    {
        private readonly float duration;

        public BGMStop(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(1, 0.0f);
        }

        public override ExecutionType Type => duration <= 0 ? ExecutionType.Instant : ExecutionType.Asynchronous;
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            var task = Module.AudioManager.StopBGM(duration);
            if (task != null)
                yield return task;
        }
    }
}