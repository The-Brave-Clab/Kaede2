using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Scenario.Entities
{
    public class SpriteEntity : ScenarioModule.Entity, IStateSavable<CommonResourceState>
    {
        public string resourceName;

        private Image image = null;
        private RectTransform rectTransform = null;

        protected override void Awake()
        {
            base.Awake();
            image = GetComponent<Image>();
            rectTransform = (RectTransform) transform;
        }

        public override Color GetColor()
        {
            return image.color;
        }

        public override void SetColor(Color color)
        {
            image.color = color;
        }

        protected override Vector3 TransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x * ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        protected override Vector3 UntransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x / ScreenWidthScalar, vec.y, vec.z);
            return result;
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
                Debug.LogError("Applying state to wrong sprite!");
                return;
            }

            RestoreTransformState(state.transform);
        }
    }
}