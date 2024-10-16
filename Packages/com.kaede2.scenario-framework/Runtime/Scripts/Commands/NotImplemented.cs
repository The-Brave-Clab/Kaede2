﻿using System.Collections;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class NotImplemented : Command
    {
        public NotImplemented(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Execute()
        {
            Debug.LogWarning($"Not Implemented Command {OriginalArg(0)}");

            if (!ScenarioRunMode.Args.TestMode) yield break;
            ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.NotImplemented);
            yield return new WaitForSeconds(1.0f);
        }
    }
}