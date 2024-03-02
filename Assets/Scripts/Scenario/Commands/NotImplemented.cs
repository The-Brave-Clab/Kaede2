﻿using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class NotImplemented : ScenarioModule.Command
    {
        public NotImplemented(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Debug.LogWarning($"Not Implemented Command {OriginalArg(0)}");
            yield break;
        }
    }
}