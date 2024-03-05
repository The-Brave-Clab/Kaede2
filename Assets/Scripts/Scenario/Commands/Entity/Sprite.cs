﻿using System.Collections;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Commands
{
    public class Sprite : ScenarioModule.Command
    {
        private readonly string resourceName;
        private readonly string objName;
        private readonly int layer;
        private readonly Vector2 position;
        private readonly float scale;
        private readonly float duration;
        private readonly float alpha;
        private readonly bool wait;

        private SpriteEntity entity;

        public Sprite(ScenarioModule module, string[] arguments) : base(module, arguments)
        {
            var split = OriginalArg(1).Split(":");
            resourceName = split[0];
            objName = split[1];
            layer = Arg(2, 0);
            position = new Vector2(Arg(3, 0.0f), Arg(4, 0.0f));
            scale = Arg(5, 1.0f);
            duration = Arg(6, 0.0f);
            alpha = Arg(7, 1.0f);
            wait = Arg(8, true);
        }

        public override ExecutionType Type => ExecutionTypeBasedOnWaitAndDuration(wait, duration);
        public override float ExpectedExecutionTime => duration;

        public override IEnumerator Setup()
        {
            var entities = Object.FindObjectsByType<SpriteEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
            if (entity == null)
            {
                if (!Module.ScenarioResource.sprites.TryGetValue(resourceName, out var sprite))
                {
                    Debug.LogError($"Sprite {resourceName} not found");
                    yield break;
                }

                entity = CreateSprite(objName, resourceName, sprite);
            }

            entity.transform.localScale = Vector3.one * scale;
            entity.Position = new Vector3(position.x, position.y, layer);

            entity.gameObject.SetActive(true);

            if (duration == 0)
            {
                var originalColor = entity.GetColor();
                entity.SetColor(new(originalColor.r, originalColor.g, originalColor.b, alpha));
                yield break;
            }

            yield return entity.ColorAlpha(entity.GetColor(), 0, alpha, duration, false);
        }

        public static SpriteEntity CreateSprite(string objectName, string resourceName, UnityEngine.Sprite sprite)
        {
            var newSprite = Object.Instantiate(UIManager.Instance.emptyUIObjectPrefab, UIManager.Instance.spriteCanvas.transform, false);
            newSprite.name = objectName;
            var image = newSprite.AddComponent<Image>();
            image.sprite = sprite;
            var entity = newSprite.AddComponent<SpriteEntity>();
            entity.resourceName = resourceName;
            entity.SetColor(new(1, 1, 1, 0));
            var rectTransform = newSprite.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            return entity;
        }
    }
}