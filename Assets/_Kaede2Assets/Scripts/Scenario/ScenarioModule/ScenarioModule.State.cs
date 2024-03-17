using System;
using System.Collections.Generic;
using System.Linq;
using biscuit.Scenario.Effect;
using Kaede2.Scenario.Audio;
using Kaede2.Scenario.Commands;
using Kaede2.Scenario.Entities;
using Kaede2.Scenario.UI;
using UnityEngine;
using Object = UnityEngine.Object;
using Sprite = Kaede2.Scenario.Commands.Sprite;

namespace Kaede2.Scenario
{
    public partial class ScenarioModule
    {
        public ScenarioState GetState()
        {
            return new()
            {
                currentCommandIndex = currentCommandIndex,
                initialized = Initialized,
                actorAutoDelete = ActorAutoDelete,
                lipSync = LipSync,

                uiOn = UIManager.Instance.uiCanvas.gameObject.activeSelf,
                cameraOn = UIManager.Instance.contentCanvas.gameObject.activeSelf,
                cameraPosition = UIManager.CameraPos,
                cameraScale = UIManager.CameraScale,

                actors = Live2DActorEntity.AllActors.Select(c => c.GetState()).ToList(),
                sprites = UIManager.Instance.spriteCanvas.GetComponentsInChildren<SpriteEntity>().Select(s => s.GetState()).ToList(),
                backgrounds = UIManager.Instance.backgroundCanvas.GetComponentsInChildren<BackgroundEntity>().Select(b => b.GetState()).ToList(),
                stills = UIManager.Instance.stillCanvas.GetComponentsInChildren<BackgroundEntity>().Select(b => b.GetState()).ToList(),
                animationPrefabs = FindObjectsByType<AnimationPrefabEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(p => p.GetState()).Where(s => s != null).ToList(),
                caption = UIManager.Instance.CaptionBox.GetState(),
                messageBox = UIManager.Instance.MessageBox.GetState(),
                fade = UIManager.Instance.fade.GetState(),
                audio = AudioManager.Instance.GetState()
            };
        }

        public void RestoreState(ScenarioState state)
        {
            void CleanAndRestoreStates<T>(Transform parent, List<T> states, Action<T> restoreState) where T : State<T>
            {
                foreach (var o in parent)
                {
                    Transform t = (Transform)o;
                    Destroy(t.gameObject);
                }

                if (states == null) return;

                foreach (var s in states)
                {
                    restoreState(s);
                }
            }

            void RestoreActorState(ActorState actorState)
            {
                if (!ScenarioResource.actors.TryGetValue(actorState.modelName, out var asset))
                {
                    Debug.LogError($"Live2D model {actorState.modelName} not found");
                    return;
                }

                var entity = ActorSetup.CreateActor(actorState.transform.position, actorState.objectName, asset);
                entity.RestoreState(actorState);
            }

            void RestoreSpriteState(CommonResourceState spriteState)
            {
                if (!ScenarioResource.sprites.TryGetValue(spriteState.resourceName, out var sprite))
                {
                    Debug.LogError($"Sprite {spriteState.resourceName} not found");
                    return;
                }

                var entity = Sprite.CreateSprite(spriteState.objectName, spriteState.resourceName, sprite);
                entity.RestoreState(spriteState);
            }

            void RestoreBackgroundState(CommonResourceState backgroundState)
            {
                if (!ScenarioResource.backgrounds.TryGetValue(backgroundState.resourceName, out var tex))
                {
                    Debug.LogError($"Background texture {backgroundState.resourceName} not found");
                    return;
                }

                var entity = BG.CreateBackground(UIManager.Instance.backgroundCanvas.transform, backgroundState.objectName, backgroundState.resourceName, tex);
                entity.RestoreState(backgroundState);
            }

            void RestoreStillState(CommonResourceState stillState)
            {
                if (!ScenarioResource.stills.TryGetValue(stillState.resourceName, out var tex))
                {
                    Debug.LogError($"Background texture {stillState.resourceName} not found");
                    return;
                }

                var entity = BG.CreateBackground(UIManager.Instance.stillCanvas.transform, stillState.objectName, stillState.resourceName, tex);
                entity.RestoreState(stillState);
            }

            CleanAndRestoreStates(UIManager.Instance.live2DCanvas.transform, state.actors, RestoreActorState);
            CleanAndRestoreStates(UIManager.Instance.spriteCanvas.transform, state.sprites, RestoreSpriteState);
            CleanAndRestoreStates(UIManager.Instance.backgroundCanvas.transform, state.backgrounds, RestoreBackgroundState);
            CleanAndRestoreStates(UIManager.Instance.stillCanvas.transform, state.stills, RestoreStillState);

            // clean and restore animation prefab states
            foreach (var animPrefab in FindObjectsByType<AnimationPrefabEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Destroy(animPrefab.gameObject);
            }

            foreach (var animPrefabState in state.animationPrefabs)
            {
                var prefab = EffectPrefabs.Find(p =>
                    p.name.Equals(animPrefabState.prefabName, StringComparison.InvariantCultureIgnoreCase));
                if (prefab == null)
                {
                    Debug.LogError($"Animation Prefab {animPrefabState.prefabName} not found");
                    continue;
                }

                GameObject instantiated = Instantiate(prefab);
                instantiated.name = animPrefabState.objectName;
                AnimationPrefabEntity entity = instantiated.AddComponent<AnimationPrefabEntity>();
                entity.prefabName = animPrefabState.prefabName;

                if (animPrefabState.isTransform)
                {
                    CharacterTransformController controller = instantiated.GetComponent<CharacterTransformController>();
                    controller.Setup(animPrefabState.id);
                }
                else
                {
                    Transform animTransform = instantiated.transform;
                    animTransform.localScale = Vector3.one * animPrefabState.scale;
                    entity.Position = animPrefabState.position;
                }
            }

            UIManager.Instance.CaptionBox.RestoreState(state.caption);
            UIManager.Instance.MessageBox.RestoreState(state.messageBox);
            UIManager.Instance.fade.RestoreState(state.fade);
            AudioManager.Instance.RestoreState(state.audio);

            currentCommandIndex = state.currentCommandIndex;
            Initialized = state.initialized;
            ActorAutoDelete = state.actorAutoDelete;
            LipSync = state.lipSync;

            UIManager.Instance.uiCanvas.gameObject.SetActive(state.uiOn);
            UIManager.Instance.contentCanvas.gameObject.SetActive(state.cameraOn);
            UIManager.CameraPos = state.cameraPosition;
            UIManager.CameraScale = state.cameraScale;
        }
    }
}
