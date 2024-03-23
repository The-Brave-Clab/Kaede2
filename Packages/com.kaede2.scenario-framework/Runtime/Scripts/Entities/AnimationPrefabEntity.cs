using biscuit.Scenario.Effect;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Entities
{
    public class AnimationPrefabEntity : Entity, IStateSavable<AnimationPrefabState>
    {
        public string prefabName;

        public override Vector3 Position
        {
            get => UntransformVector(transform.localPosition);
            set => transform.localPosition = TransformVector(value);
        }

        protected override Vector3 TransformVector(Vector3 vec)
        {
            Vector2 v = new Vector2(vec.x * ScreenWidthScalar, vec.y) * 2.0f / 1080.0f;
            return new Vector3(v.x, v.y, vec.z);
        }

        protected override Vector3 UntransformVector(Vector3 vec)
        {
            Vector2 v = new Vector2(vec.x, vec.y) * 1080.0f / 2.0f;
            v.x /= ScreenWidthScalar;
            return new Vector3(v.x, v.y, vec.z);
        }

        public AnimationPrefabState GetState()
        {
            if (TryGetComponent<EffectDestroyer>(out _))
                return null;

            if (TryGetComponent<CharacterTransformController>(out var transformController))
            {
                return new()
                {
                    objectName = gameObject.name,
                    prefabName = prefabName,
                    isTransform = true,
                    id = transformController.CharacterID
                };
            }

            return new()
            {
                objectName = gameObject.name,
                prefabName = prefabName,
                position = Position,
                scale = transform.localScale.x,
                isTransform = false,
                id = transformController.CharacterID
            };
        }

        public void RestoreState(AnimationPrefabState state)
        {
            if (TryGetComponent<EffectDestroyer>(out _))
            {
                Debug.LogError($"Animation Prefab with EffectDestroyer cannot be restored");
                return;
            }

            if (state.isTransform)
            {
                if (!TryGetComponent<CharacterTransformController>(out var transformController))
                {
                    Debug.LogError($"Animation Prefab {state.objectName} is not a transform prefab");
                    return;
                }

                gameObject.name = state.objectName;
                prefabName = state.prefabName;
                transformController.Setup(state.id);
            }
            else
            {
                if (TryGetComponent<CharacterTransformController>(out _))
                {
                    Debug.LogError($"Animation Prefab {state.objectName} is a transform prefab");
                    return;
                }
    
                gameObject.name = state.objectName;
                prefabName = state.prefabName;
                Position = state.position;
                transform.localScale = Vector3.one * state.scale;
            }
        }
    }
}