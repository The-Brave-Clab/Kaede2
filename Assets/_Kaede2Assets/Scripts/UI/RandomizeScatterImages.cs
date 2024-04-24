using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Kaede2.UI
{
    public class RandomizeScatterImages : MonoBehaviour
    {
        [SerializeField]
        private AlbumExtraInfo availableImages;

        [SerializeField]
        private RectTransform scatterArea;

        [SerializeField]
        private Vector2 scaleRange = new Vector2(0.3f, 0.7f);

        [SerializeField]
        private Vector2 rotationRangeInDegrees = new Vector2(-10, 10);

        [SerializeField]
        private float imageBorder = 50;

        private readonly Vector2 imageSize = new Vector2(1920, 1080);

        private ResourceLoader.LoadAddressableHandle<Sprite>[] handles;

        private float CalculateGridSize()
        {
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

        private MasterAlbumInfo.AlbumInfo[] RandomImages(int count)
        {
            // no duplicate images
            var illusts = availableImages.list.Where(i => i.is16by9).OrderBy(_ => Random.value).ToArray();
            var result = new MasterAlbumInfo.AlbumInfo[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = MasterAlbumInfo.Instance.albumInfo.First(info => info.AlbumName == illusts[i].name);
            }
            return result;
        }

        private struct Img
        {
            public Vector2 Position;
            public float Rotation;
            public float Scale;
            public string AlbumName;
        }

        private IEnumerator Start()
        {
            var positions = GridPositions();
            var imageCount = positions.Count;
            var imageInfo = RandomImages(imageCount);
            var images = new List<Img>();
            for (var i = 0; i < imageCount; i++)
            {
                var position = positions[i];
                var rotation = Random.Range(rotationRangeInDegrees.x, rotationRangeInDegrees.y);
                var scale = Random.Range(scaleRange.x, scaleRange.y);
                images.Add(new Img
                {
                    Position = position,
                    Rotation = rotation,
                    Scale = scale,
                    AlbumName = imageInfo[i].AlbumName
                });
            }

            var group = new CoroutineGroup();

            handles = new ResourceLoader.LoadAddressableHandle<Sprite>[imageCount];
            for (var i = 0; i < imageCount; i++)
            {
                handles[i] = ResourceLoader.LoadIllustration(images[i].AlbumName);
                group.Add(handles[i].Send());
            }

            yield return group.WaitForAll();

            // var shuffledImages = images.OrderBy(_ => Random.value).ToList();
            var orderedImages = images
                .OrderBy(_ => Random.value)
                // .OrderBy(i => Mathf.FloorToInt(i.Scale * 10))
                // .ThenByDescending(i => i.Position.y)
                .ToList();

            for (var i = 0; i < orderedImages.Count; i++)
            {
                var image = orderedImages[i];
                var backgroundImage = new GameObject(image.AlbumName);
                backgroundImage.transform.SetParent(scatterArea, false);

                var backgroundRectTransform = backgroundImage.AddComponent<RectTransform>();
                backgroundRectTransform.sizeDelta = imageSize + Vector2.one * imageBorder;
                backgroundRectTransform.anchoredPosition = image.Position;
                backgroundRectTransform.localRotation = Quaternion.Euler(0, 0, image.Rotation);
                backgroundRectTransform.localScale = new Vector3(image.Scale, image.Scale, 1);

                var background = backgroundImage.AddComponent<Image>();
                background.color = Color.white;

                var gameObject = new GameObject(image.AlbumName);
                gameObject.transform.SetParent(backgroundImage.transform, false);

                var rectTransform = gameObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = imageSize;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                var imageComponent = gameObject.AddComponent<Image>();
                imageComponent.sprite = handles[i].Result;
            }
        }

        private void OnDestroy()
        {
            if (handles != null)
            {
                foreach (var handle in handles)
                {
                    handle.Dispose();
                }
            }
        }
    }
}
