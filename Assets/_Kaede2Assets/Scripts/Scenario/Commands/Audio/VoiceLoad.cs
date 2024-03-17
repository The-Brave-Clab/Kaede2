using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class VoiceLoad : Command
    {
        private readonly string voiceName;

        public VoiceLoad(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            voiceName = Arg<string>(1);
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.Voices.ContainsKey(voiceName))
                yield break;

            var loadHandle = ResourceLoader.LoadScenarioVoice(Module.ScenarioName, voiceName);
            Module.RegisterLoadHandle(loadHandle);
            yield return loadHandle.Send();
        }
    }
}