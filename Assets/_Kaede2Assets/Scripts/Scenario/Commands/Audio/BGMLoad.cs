﻿using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Utils;

namespace Kaede2.Scenario.Commands
{
    public class BGMLoad : Command
    {
        private readonly string bgmName;

        public BGMLoad(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            bgmName = Arg<string>(1);
        }

        public override ExecutionType Type => ExecutionType.Synchronous;
        public override float ExpectedExecutionTime => -1;

        public override IEnumerator Execute()
        {
            if (Module.ScenarioResource.BackgroundMusics.ContainsKey(bgmName))
                yield break;

            var loadHandle = ResourceLoader.LoadScenarioBackgroundMusic(bgmName);
            Module.RegisterLoadHandle(loadHandle);
            yield return loadHandle.Send();
        }
    }
}