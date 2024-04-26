using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kaede2.Scenario.Framework
{
    public abstract class State<T> : IEquatable<T>
    {
        public abstract T Copy();
        public abstract bool Equals(T other);
    }

    public interface IStateSavable<T> where T : State<T>
    {
        T GetState();
        void RestoreState(T state);
    }

    [Serializable]
    public class ScenarioState : State<ScenarioState>
    {
        public int currentCommandIndex = -1;
        public bool actorAutoDelete = false;
        public bool lipSync = true;

        public bool uiOn = false;
        public bool cameraOn = false;
        public Vector2 cameraPosition = Vector2.zero;
        public float cameraScale = 1.0f;

        public List<ActorState> actors = new();
        public List<CommonResourceState> sprites = new();
        public List<CommonResourceState> backgrounds = new();
        public List<CommonResourceState> stills = new();
        public List<AnimationPrefabState> animationPrefabs = new();
        public CaptionState caption = new();
        public MessageBoxState messageBox = new();
        public FadeState fade = new();
        public AudioState audio = new();

        public override ScenarioState Copy()
        {
            return new()
            {
                currentCommandIndex = currentCommandIndex,
                actorAutoDelete = actorAutoDelete,
                lipSync = lipSync,
                uiOn = uiOn,
                cameraOn = cameraOn,
                cameraPosition = cameraPosition,
                cameraScale = cameraScale,
                actors = actors.Select(a => a.Copy()).ToList(),
                sprites = sprites.Select(s => s.Copy()).ToList(),
                backgrounds = backgrounds.Select(b => b.Copy()).ToList(),
                stills = stills.Select(s => s.Copy()).ToList(),
                animationPrefabs = animationPrefabs.Select(a => a.Copy()).ToList(),
                caption = caption.Copy(),
                messageBox = messageBox.Copy(),
                fade = fade.Copy(),
                audio = audio.Copy(),
            };
        }

        public override bool Equals(ScenarioState other)
        {
            if (other is null) return false;

            return currentCommandIndex == other.currentCommandIndex &&
                   actorAutoDelete == other.actorAutoDelete &&
                   lipSync == other.lipSync &&
                   uiOn == other.uiOn &&
                   cameraOn == other.cameraOn &&
                   cameraPosition.Equals(other.cameraPosition) &&
                   cameraScale.Equals(other.cameraScale) &&
                   actors.SequenceEqual(other.actors) &&
                   sprites.SequenceEqual(other.sprites) &&
                   backgrounds.SequenceEqual(other.backgrounds) &&
                   stills.SequenceEqual(other.stills) &&
                   animationPrefabs.SequenceEqual(other.animationPrefabs) &&
                   caption.Equals(other.caption) &&
                   messageBox.Equals(other.messageBox) &&
                   fade.Equals(other.fade) &&
                   audio.Equals(other.audio);
        }
    }

    [Serializable]
    public class EntityTransform : State<EntityTransform>
    {
        public bool enabled = true;
        public Vector3 position = Vector3.zero;
        public float angle = 0.0f;
        public float scale = 1.0f;
        public Vector2 pivot = new(0.5f, 0.5f);
        public Color color = Color.white;

        public override EntityTransform Copy()
        {
            return new()
            {
                enabled = enabled,
                position = position,
                angle = angle,
                scale = scale,
                pivot = pivot,
                color = color
            };
        }

        public override bool Equals(EntityTransform other)
        {
            if (other is null) return false;

            return enabled == other.enabled &&
                   position.Equals(other.position) &&
                   angle.Equals(other.angle) &&
                   scale.Equals(other.scale) &&
                   pivot.Equals(other.pivot) &&
                   color.Equals(other.color);
        }
    }

    [Serializable]
    public class ActorState : State<ActorState>
    {
        public string objectName = "";
        public string modelName = "";
        public string currentMotion = "";
        public string currentFaceMotion = "";
        public bool currentMotionLoop = false;

        public int layer;

        public bool hidden = false;

        public bool eyeBlink = true;
        public bool manualEyeOpen = true;

        public List<string> mouthSynced = new();

        public Vector2 faceAngle;
        public float bodyAngle;

        public float addEye;
        public float absoluteEye;

        public EntityTransform transform = new();

        public override ActorState Copy()
        {
            return new()
            {
                objectName = string.Copy(objectName),
                modelName = string.Copy(modelName),
                currentMotion = string.Copy(currentMotion),
                currentFaceMotion = string.Copy(currentFaceMotion),
                currentMotionLoop = currentMotionLoop,
                layer = layer,
                hidden = hidden,
                eyeBlink = eyeBlink,
                manualEyeOpen = manualEyeOpen,
                mouthSynced = mouthSynced.Select(string.Copy).ToList(),
                faceAngle = faceAngle,
                bodyAngle = bodyAngle,
                addEye = addEye,
                absoluteEye = absoluteEye,
                transform = transform.Copy()
            };
        }

        public override bool Equals(ActorState other)
        {
            if (other is null) return false;

            return string.Equals(objectName, other.objectName) &&
                   string.Equals(modelName, other.modelName) &&
                   string.Equals(currentMotion, other.currentMotion) &&
                   string.Equals(currentFaceMotion, other.currentFaceMotion) &&
                   currentMotionLoop == other.currentMotionLoop &&
                   layer == other.layer &&
                   hidden == other.hidden &&
                   eyeBlink == other.eyeBlink &&
                   manualEyeOpen == other.manualEyeOpen &&
                   mouthSynced.SequenceEqual(other.mouthSynced) &&
                   faceAngle.Equals(other.faceAngle) &&
                   bodyAngle.Equals(other.bodyAngle) &&
                   addEye.Equals(other.addEye) &&
                   absoluteEye.Equals(other.absoluteEye) &&
                   transform.Equals(other.transform);
        }
    }

    [Serializable]
    public class CommonResourceState : State<CommonResourceState>
    {
        public string objectName = "";
        public string resourceName = "";

        public EntityTransform transform = new();

        public override CommonResourceState Copy()
        {
            return new()
            {
                objectName = string.Copy(objectName),
                resourceName = string.Copy(resourceName),
                transform = transform.Copy()
            };
        }

        public override bool Equals(CommonResourceState other)
        {
            if (other is null) return false;

            return string.Equals(objectName, other.objectName) &&
                   string.Equals(resourceName, other.resourceName) &&
                   transform.Equals(other.transform);
        }
    }

    [Serializable]
    public class CaptionState : State<CaptionState>
    {
        public bool enabled = false;
        public Color boxColor = Color.white;
        public string text = "";
        public float textAlpha = 1.0f;

        public override CaptionState Copy()
        {
            return new()
            {
                enabled = enabled,
                boxColor = boxColor,
                text = string.Copy(text),
                textAlpha = textAlpha
            };
        }

        public override bool Equals(CaptionState other)
        {
            if (other is null) return false;

            return enabled == other.enabled &&
                   boxColor.Equals(other.boxColor) &&
                   string.Equals(text, other.text) &&
                   textAlpha.Equals(other.textAlpha);
        }
    }

    [Serializable]
    public class MessageBoxState : State<MessageBoxState>
    {
        public bool enabled = false;
        public bool namePanelEnabled = true;
        public string speaker = "";
        public string message = "";

        public override MessageBoxState Copy()
        {
            return new()
            {
                enabled = enabled,
                namePanelEnabled = namePanelEnabled,
                speaker = string.Copy(speaker),
                message = string.Copy(message)
            };
        }

        public override bool Equals(MessageBoxState other)
        {
            if (other is null) return false;

            return enabled == other.enabled &&
                   namePanelEnabled == other.namePanelEnabled &&
                   string.Equals(speaker, other.speaker) &&
                   string.Equals(message, other.message);
        }
    }

    [Serializable]
    public class FadeState : State<FadeState>
    {
        public float progress = 1.0f;

        public override FadeState Copy()
        {
            return new() { progress = progress };
        }

        public override bool Equals(FadeState other)
        {
            if (other is null) return false;

            return progress.Equals(other.progress);
        }
    }

    [Serializable]
    public class AudioState : State<AudioState>
    {
        public bool bgmPlaying = false;
        public string bgmName = "";
        public float bgmVolume = 1.0f;

        public override AudioState Copy()
        {
            return new()
            {
                bgmPlaying = bgmPlaying,
                bgmName = string.Copy(bgmName),
                bgmVolume = bgmVolume
            };
        }

        public override bool Equals(AudioState other)
        {
            if (other is null) return false;

            return bgmPlaying == other.bgmPlaying &&
                   string.Equals(bgmName, other.bgmName) &&
                   bgmVolume.Equals(other.bgmVolume);
        }
    }

    [Serializable]
    public class AnimationPrefabState : State<AnimationPrefabState>
    {
        public string objectName = "";
        public string prefabName = "";
        public Vector2 position = Vector2.zero;
        public float scale = 1.0f;
        public bool isTransform = false;
        public CharacterId id = CharacterId.Unknown;

        public override AnimationPrefabState Copy()
        {
            return new()
            {
                objectName = string.Copy(objectName),
                prefabName = string.Copy(prefabName),
                position = position,
                scale = scale,
                isTransform = isTransform,
                id = id
            };
        }

        public override bool Equals(AnimationPrefabState other)
        {
            if (other is null) return false;

            return string.Equals(objectName, other.objectName) &&
                   string.Equals(prefabName, other.prefabName) &&
                   position.Equals(other.position) &&
                   scale.Equals(other.scale) &&
                   isTransform == other.isTransform &&
                    id == other.id;
        }
    }
}