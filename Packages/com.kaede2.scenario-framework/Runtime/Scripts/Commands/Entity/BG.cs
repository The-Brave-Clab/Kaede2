using System.Collections;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Commands
{
    public class BG : Command
    {
        private readonly string resourceName;
        private readonly string objName;
        private readonly Vector2 position;
        private readonly float scale;
        private readonly int layer;

        public BG(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            resourceName = Arg(1, "");
            objName = OriginalArg(1);
            position = new Vector2(Arg(2, 0.0f), Arg(3, 0.0f));
            scale = Arg(4, 1.0f);
            layer = Arg(5, 0);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override void Setup()
        {
            var targetPosition = new Vector3(position.x, position.y, layer);
            var targetScale = scale * Vector3.one;

            if (!Module.ScenarioResource.Backgrounds.TryGetValue(resourceName, out var tex))
            {
                Debug.LogError($"Background texture {resourceName} not found");
                if (ScenarioRunMode.Args.TestMode)
                    ScenarioRunMode.FailTest(ScenarioRunMode.FailReason.ResourceNotFound);
                return;
            }

            var entity = Module.UIController.CreateBackground(objName, resourceName, tex);

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