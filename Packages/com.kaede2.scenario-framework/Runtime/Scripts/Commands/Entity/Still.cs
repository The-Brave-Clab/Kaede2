using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class Still : Command
    {
        private readonly string resourceName;
        private readonly string objName;
        private readonly int layer;
        private readonly Vector2 position;
        private readonly float scale;

        public Still(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            resourceName = Arg(1, ":").Split(":")[0];
            objName = OriginalArg(1, ":").Split(":")[1];
            layer = Arg(2, 0);
            position = new Vector2(Arg(3, 0.0f), Arg(4, 0.0f));
            scale = Arg(5, 1.0f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            var targetPosition = new Vector3(position.x, position.y, layer);
            var targetScale = scale * Vector3.one;

            if (!Module.ScenarioResource.Stills.TryGetValue(resourceName, out var tex))
            {
                Debug.LogError($"Still texture {resourceName} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
                return;
            }

            var entity = Module.UIController.CreateStill(objName, resourceName, tex);

            entity.Position = targetPosition;
            var rectTransform = entity.transform as RectTransform;
            rectTransform!.localScale = targetScale;
            entity.gameObject.SetActive(true);
        }

        public override IEnumerator Execute()
        {
            yield break;
        }
    }
}