using System.Collections;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class Still : Command
    {
        private readonly string resourceName;
        private readonly string objName;
        private readonly int layer;
        private readonly Vector2 position;
        private readonly float scale;

        private BackgroundEntity entity;

        public Still(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            resourceName = Arg(1, ":").Split(":")[0];
            objName = OriginalArg(1, ":").Split(":")[1];
            layer = Arg(2, 0);
            position = new Vector2(Arg(3, 0.0f), Arg(4, 0.0f));
            scale = Arg(5, 1.0f);
        }

        public override ExecutionType Type => ExecutionType.Instant;
        public override float ExpectedExecutionTime => 0;

        public override IEnumerator Setup()
        {
            var entities = Object.FindObjectsByType<BackgroundEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (entities == null || entities.Length == 0)
            {
                entity = null;
                yield break;
            }

            foreach (var e in entities)
            {
                if (e.name == objName)
                {
                    entity = e;
                    yield break;
                }
            }

            entity = null;
        }

        public override IEnumerator Execute()
        {
            var targetPosition = new Vector3(position.x, position.y, layer);
            var targetScale = scale * Vector3.one;

            if (entity == null)
            {
                if (!Module.ScenarioResource.Stills.TryGetValue(resourceName, out var tex))
                {
                    Debug.LogError($"Still texture {resourceName} not found");
                    yield break;
                }

                var newStill = Object.Instantiate(Module.UIManager.backgroundPrefab, Module.UIManager.stillCanvas.transform, false);
                entity = newStill.GetComponent<BackgroundEntity>();
                newStill.name = objName;
                entity.resourceName = resourceName;
                entity.Canvas = Module.UIManager.stillCanvas.transform as RectTransform;
                entity.SetImage(tex);
            }


            entity.Position = targetPosition;
            var rectTransform = entity.transform as RectTransform;
            rectTransform!.localScale = targetScale;
            entity.gameObject.SetActive(true);
        }
    }
}