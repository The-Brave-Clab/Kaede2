﻿using System.Collections;

namespace Kaede2.Scenario.Commands
{
    public class ActorAutoMouth : ScenarioModule.Command
    {
        private readonly bool value;

        public ActorAutoMouth(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            value = Arg(1, true);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Module.LipSync = value;
            yield break;
        }
    }
}