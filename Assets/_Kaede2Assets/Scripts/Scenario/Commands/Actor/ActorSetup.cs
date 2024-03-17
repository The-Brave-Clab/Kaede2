using System.Collections;
using Kaede2.Scenario.Base;
using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class ActorSetup : Command
    {
        private readonly string modelName;
        private readonly string readableName;
        private readonly float xPos;

        private Live2DActorEntity entity;

        public ActorSetup(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            modelName = Arg(1, "");
            readableName = OriginalArg(1);
            xPos = Arg(2, 0.0f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            var entities = Object.FindObjectsByType<Live2DActorEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (entities == null || entities.Length == 0)
            {
                entity = null;
                yield break;
            }

            foreach (var e in entities)
            {
                if (e.Assets.name == modelName)
                {
                    entity = e;
                    yield break;
                }
            }

            entity = null;
        }

        public override IEnumerator Execute()
        {
            if (entity != null) yield break;
            if (!Module.ScenarioResource.Actors.TryGetValue(modelName, out var asset))
            {
                Debug.LogError($"Live2D model {modelName} not found");
                yield break;
            }

            Module.UIController.CreateActor(new(xPos, 0), readableName, asset);
        }
    }
}