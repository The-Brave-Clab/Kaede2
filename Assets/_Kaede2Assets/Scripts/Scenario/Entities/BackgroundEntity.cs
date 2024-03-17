using Kaede2.Scenario.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Entities
{
    public class BackgroundEntity : Entity, IStateSavable<CommonResourceState>
    {
        public string resourceName;

        public RawImage image;

        public RectTransform Canvas { get; set; }

        private void Update()
        {
            Resize();
        }

        private void Resize()
        {
            var rectTransform = image.rectTransform;
            var pixelRect = Canvas.rect;

            // if we are in fixed 16:9 mode, adjust pixelRect first
            if (GameSettings.Fixed16By9)
            {
                if (pixelRect.width * 9 > pixelRect.height * 16)
                {
                    // preserve height
                    pixelRect.width = pixelRect.height * 16.0f / 9.0f;
                }
            }


            var texture = image.texture;
            if (texture == null)
            {
                rectTransform.sizeDelta = new Vector2(pixelRect.width, pixelRect.height);
                return;
            }
            if (texture.width * pixelRect.height > pixelRect.width * texture.height)
            {
                // preserve height
                rectTransform.sizeDelta = new Vector2(pixelRect.height * texture.width / texture.height, pixelRect.height);
            }
            else
            {
                // preserve width
                rectTransform.sizeDelta = new Vector2(pixelRect.width, pixelRect.width * texture.height / texture.width);
            }
        }

        public void SetImage(Texture2D bg)
        {
            image.texture = bg;
            Resize();
        }

        public override Color GetColor()
        {
            return image.color;
        }

        public override void SetColor(Color color)
        {
            image.color = color;
        }

        public CommonResourceState GetState()
        {
            return new()
            {
                objectName = gameObject.name,
                resourceName = resourceName,
                transform = GetTransformState()
            };
        }

        public void RestoreState(CommonResourceState state)
        {
            if (name != state.objectName || resourceName != state.resourceName)
            {
                Debug.LogError("Applying state to wrong background!");
                return;
            }

            RestoreTransformState(state.transform);
        }
    }
}
