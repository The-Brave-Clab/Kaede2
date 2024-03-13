using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kaede2.Scenario
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
        public bool initialized = false;
        public bool actorAutoDelete = false;
        public bool lipSync = true;

        public List<ActorState> actors = new();
        public List<CommonResourceState> sprites = new();
        public List<CommonResourceState> backgrounds = new();
        public List<CommonResourceState> stills = new();
        public CaptionState caption = new();
        public MessageBoxState messageBox = new();
        public FadeState fade = new();
        public AudioState audio = new();

        public override ScenarioState Copy()
        {
            return new()
            {
                currentCommandIndex = currentCommandIndex,
                initialized = initialized,
                actorAutoDelete = actorAutoDelete,
                lipSync = lipSync,
                actors = actors.Select(a => a.Copy()).ToList(),
                sprites = sprites.Select(s => s.Copy()).ToList(),
                backgrounds = backgrounds.Select(b => b.Copy()).ToList(),
                stills = stills.Select(s => s.Copy()).ToList(),
                caption = caption.Copy(),
                messageBox = messageBox.Copy(),
                fade = fade.Copy(),
                audio = audio.Copy()
            };
        }

        public override bool Equals(ScenarioState other)
        {
            if (other is null) return false;

            return currentCommandIndex == other.currentCommandIndex &&
                   initialized == other.initialized &&
                   actorAutoDelete == other.actorAutoDelete &&
                   lipSync == other.lipSync &&
                   actors.SequenceEqual(other.actors) &&
                   sprites.SequenceEqual(other.sprites) &&
                   backgrounds.SequenceEqual(other.backgrounds) &&
                   stills.SequenceEqual(other.stills) &&
                   caption.Equals(other.caption) &&
                   messageBox.Equals(other.messageBox) &&
                   fade.Equals(other.fade) &&
                   audio.Equals(other.audio);
        }
    }

    [Serializable]
    public class EntityTransform : State<EntityTransform>
    {
        public bool enabled;
        public Vector3 position;
        public float angle;
        public float scale;
        public Vector2 pivot;
        public Color color;

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

        public int layer;

        public bool hidden;

        public bool eyeBlink;
        public bool manualEyeOpen;

        public List<string> mouthSynced = new();

        public Vector2 faceAngle;
        public float bodyAngle;

        public float addEye;

        public EntityTransform transform = new();

        public override ActorState Copy()
        {
            return new()
            {
                objectName = string.Copy(objectName),
                modelName = string.Copy(modelName),
                currentMotion = string.Copy(currentMotion),
                currentFaceMotion = string.Copy(currentFaceMotion),
                layer = layer,
                hidden = hidden,
                eyeBlink = eyeBlink,
                manualEyeOpen = manualEyeOpen,
                mouthSynced = mouthSynced.Select(string.Copy).ToList(),
                faceAngle = faceAngle,
                bodyAngle = bodyAngle,
                addEye = addEye,
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
                   layer == other.layer &&
                   hidden == other.hidden &&
                   eyeBlink == other.eyeBlink &&
                   manualEyeOpen == other.manualEyeOpen &&
                   mouthSynced.SequenceEqual(other.mouthSynced) &&
                   faceAngle.Equals(other.faceAngle) &&
                   bodyAngle.Equals(other.bodyAngle) &&
                   addEye.Equals(other.addEye) &&
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
        public string speaker = "";
        public string message = "";

        public override MessageBoxState Copy()
        {
            return new()
            {
                enabled = enabled,
                speaker = string.Copy(speaker),
                message = string.Copy(message)
            };
        }

        public override bool Equals(MessageBoxState other)
        {
            if (other is null) return false;

            return enabled == other.enabled &&
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
}