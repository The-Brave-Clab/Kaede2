using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Kaede2.UI
{
    public class RandomizeScatterImages : MonoBehaviour
    {
        [SerializeField]
        private RandomizedImageProvider imageProvider;

        [SerializeField]
        private RectTransform scatterArea;

        [SerializeField]
        private Vector2 scaleRange = new Vector2(0.3f, 0.7f);

        [SerializeField]
        private Vector2 rotationRangeInDegrees = new Vector2(-10, 10);

        [SerializeField]
        private Vector2 imageBorder = Vector2.one * 50;

        public bool Loaded { get; private set; } = false;

        private float CalculateGridSize()
        {
            var imageSize = imageProvider.ImageSize;
            var smallestSide = Mathf.Min(imageSize.x, imageSize.y) * scaleRange.x;
            return smallestSide / Mathf.Sqrt(2);
        }

        private List<Vector2> GridPositions()
        {
            var gridSize = CalculateGridSize();

            var rect = scatterArea.rect;
            
            var result = new List<Vector2>();

            var gridStartOffset = new Vector2(Random.Range(-gridSize, 0), Random.Range(-gridSize, 0));
            for (var x = gridStartOffset.x; x < rect.width; x += gridSize)
            {
                for (var y = gridStartOffset.y; y < rect.height; y += gridSize)
                {
                    result.Add(new Vector2(x + gridSize / 2 - rect.width / 2, y + gridSize / 2 - rect.height / 2));
                }
            }

            return result;
        }

        private struct Img
        {
            public Vector2 Position;
            public float Rotation;
            public float Scale;
            public string Name;
            public Sprite Image;
        }

        private IEnumerator Start()
        {
            var positions = GridPositions();
            var imageCount = positions.Count;
            var images = Array.Empty<RandomizedImageProvider.ImageInfo>();
            yield return imageProvider.Provide(imageCount, sprites => images = sprites);
            var imageDescriptions = new List<Img>();
            for (var i = 0; i < images.Length; i++)
            {
                var position = positions[i];
                var rotation = Random.Range(rotationRangeInDegrees.x, rotationRangeInDegrees.y);
                var scale = Random.Range(scaleRange.x, scaleRange.y);
                imageDescriptions.Add(new Img
                {
                    Position = position,
                    Rotation = rotation,
                    Scale = scale,
                    Name = images[i].Name,
                    Image = images[i].Sprite
                });
            }

            // var shuffledImages = images.OrderBy(_ => Random.value).ToList();
            var orderedImages = imageDescriptions
                .OrderBy(_ => Random.value)
                // .OrderBy(i => Mathf.FloorToInt(i.Scale * 10))
                // .ThenByDescending(i => i.Position.y)
                .ToList();

            var imageSize = imageProvider.ImageSize;
            for (var i = 0; i < orderedImages.Count; i++)
            {
                var image = orderedImages[i];
                var backgroundImage = new GameObject(image.Name);
                backgroundImage.transform.SetParent(scatterArea, false);

                var backgroundRectTransform = backgroundImage.AddComponent<RectTransform>();
                backgroundRectTransform.sizeDelta = imageSize + imageBorder;
                backgroundRectTransform.anchoredPosition = image.Position;
                backgroundRectTransform.localRotation = Quaternion.Euler(0, 0, image.Rotation);
                backgroundRectTransform.localScale = new Vector3(image.Scale, image.Scale, 1);

                var background = backgroundImage.AddComponent<Image>();
                background.color = Color.white;

                var go = new GameObject(image.Name);
                go.transform.SetParent(backgroundImage.transform, false);

                var rectTransform = go.AddComponent<RectTransform>();
                rectTransform.sizeDelta = imageSize;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                var imageComponent = go.AddComponent<Image>();
                imageComponent.sprite = image.Image;
            }

            Loaded = true;
        }
    }
}
