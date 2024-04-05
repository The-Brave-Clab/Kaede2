using System.Collections;
using System.Linq;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class ActorSetup : Command
    {
        private readonly string modelName;
        private readonly string readableName;
        private readonly float xPos;

        public ActorSetup(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            modelName = Arg(1, "");
            readableName = OriginalArg(1);
            xPos = Arg(2, 0.0f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            if (!Module.ScenarioResource.Actors.TryGetValue(modelName, out var asset))
            {
                Debug.LogError($"Live2D model {modelName} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
                return;
            }

            Module.UIController.CreateActor(new(xPos, 0), readableName, asset);
        }

        public override IEnumerator Execute()
        {
            yield break;
        }
    }
}