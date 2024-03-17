using System.Collections;
using Kaede2.Scenario.Base;

namespace Kaede2.Scenario.Commands
{
    public class BGM : Command
    {
        private readonly string bgmName;
        private readonly float volume;

        public BGM(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            bgmName = Arg(1, "");
            volume = Arg(2, 1f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.AudioManager.PlayBGM(bgmName, volume);
            yield break;
        }
    }
}