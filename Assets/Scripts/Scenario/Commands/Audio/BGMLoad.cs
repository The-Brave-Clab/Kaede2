using System.Collections;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class BGMLoad : ScenarioModule.Command
    {
        private readonly string bgmName;

        public BGMLoad(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            bgmName = Arg<string>(1);
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.backgroundMusics.ContainsKey(bgmName))
                yield break;

            var loadHandle = ResourceLoader.LoadScenarioBackgroundMusic(bgmName);
            Module.RegisterLoadHandle(loadHandle);
            yield return loadHandle.Send();
        }

        public override void Undo()
        {
        }
    }
}