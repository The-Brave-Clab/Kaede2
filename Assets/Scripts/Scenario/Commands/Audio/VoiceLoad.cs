using System.Collections;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class VoiceLoad : ScenarioModule.Command
    {
        private readonly string voiceName;

        public VoiceLoad(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            voiceName = Arg<string>(1);
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.voices.ContainsKey(voiceName))
                yield break;

            var loadHandle = ResourceLoader.LoadScenarioVoice(ScenarioModule.ScenarioName, voiceName);
            Module.RegisterLoadHandle(loadHandle);
            yield return loadHandle.Send();
        }

        public override void Undo()
        {
        }
    }
}