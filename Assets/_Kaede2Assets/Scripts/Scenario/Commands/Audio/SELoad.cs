using System.Collections;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class SELoad : Command
    {
        private readonly string seName;

        public SELoad(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            seName = Arg<string>(1);
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.SoundEffects.ContainsKey(seName))
                yield break;

            var loadHandle = ResourceLoader.LoadScenarioSoundEffect(seName);
            Module.RegisterLoadHandle(loadHandle);
            yield return loadHandle.Send();
        }
    }
}