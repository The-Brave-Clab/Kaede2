using System.Collections;
using Kaede2.Scenario.Base;

namespace Kaede2.Scenario.Commands
{
    public class SE : Command
    {
        private readonly string seName;
        private readonly float volume;
        private readonly float duration;

        protected virtual bool Loop => false;

        public SE(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            seName = Arg<string>(1);
            volume = Arg(2, 1.0f);
            duration = Arg(3, 0.0f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            var task = Module.AudioManager.PlaySE(seName, volume, duration, Loop);
            if (task != null)
                yield return task;
        }
    }
}