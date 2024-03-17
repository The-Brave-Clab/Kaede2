using System.Collections;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using UnityEngine;

namespace Kaede2.Scenario.Commands
{
    public class BG : Command
    {
        private readonly string resourceName;
        private readonly string objName;
        private readonly Vector2 position;
        private readonly float scale;
        private readonly int layer;

        private BackgroundEntity entity;

        public BG(ScenarioModuleBase module, string[] arguments) : base(module, arguments)
        {
            resourceName = Arg(1, "");
            objName = OriginalArg(1);
            position = new Vector2(Arg(2, 0.0f), Arg(3, 0.0f));
            scale = Arg(4, 1.0f);
            layer = Arg(5, 0);
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
                if (!Module.ScenarioResource.Backgrounds.TryGetValue(resourceName, out var tex))
                {
                    Debug.LogError($"Background texture {resourceName} not found");
                    yield break;
                }

                entity = CreateBackground(Module, Module.UIManager.backgroundCanvas.transform, objName, resourceName, tex);
            }


            entity.Position = targetPosition;
            var rectTransform = entity.transform as RectTransform;
            rectTransform!.localScale = targetScale;
            entity.gameObject.SetActive(true);
        }

        public static BackgroundEntity CreateBackground(ScenarioModuleBase module, Transform parent, string objectName, string resourceName, Texture2D texture)
        {
            var newBG = Object.Instantiate(module.UIManager.backgroundPrefab, parent, false);
            var entity = newBG.GetComponent<BackgroundEntity>();
            newBG.name = objectName;
            entity.resourceName = resourceName;
            entity.Canvas = module.UIManager.contentCanvas.transform as RectTransform;
            entity.SetImage(texture);
            return entity;
        }
    }
}