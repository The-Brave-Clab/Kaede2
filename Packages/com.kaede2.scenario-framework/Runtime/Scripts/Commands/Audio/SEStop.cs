﻿using System.Collections;

namespace Kaede2.Scenario.Framework.Commands
{
    public class SEStop : Command
    {
        private readonly string seName;
        private readonly float duration;

        public SEStop(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            seName = Arg(1, "");
            duration = Arg(2, 0.0f);
        }

        public override ExecutionType Type => duration <= 0 ? ExecutionType.Instant : ExecutionType.Asynchronous;
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Execute()
        {
            var task = Module.AudioManager.StopSE(seName, duration);
            if (task != null)
                yield return task;
        }
    }
}