using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Entities
{
    public partial class Live2DActorEntity
    {
        public int Layer
        {
            get => layer;
            set
            {
                layer = value;
                ReorderLayers();
            }
        }
        public void StartMotion(string motionName, bool loop = false)
        {
            if (!FixMotionName(ref motionName)) return;

            var motion = motions[motionName];
            motion.setLoop(loop);
            nextMotion = motion;
            currentMotionName = motionName;
        }

        public void StartFaceMotion(string motionName)
        {
            if (!FixMotionName(ref motionName)) return;

            var motion = motions[motionName];
            faceMotionMgr.startMotion(motion);
            currentFaceMotionName = motionName;
        }

        public void SetLip(float volume, List<Live2DActorEntity> traversalBuffer = null)
        {
            MouthOpenY = volume;

            traversalBuffer ??= new List<Live2DActorEntity>();

            traversalBuffer.Add(this);
            
            foreach (var modelController in mouthSynced.Where(e => !traversalBuffer.Contains(e)))
            {
                modelController.SetLip(volume, traversalBuffer);
            }
        }

        public void AddMouthSync(Live2DActorEntity model)
        {
            if (model == this) return;
            if (mouthSynced.Contains(model)) return;
            mouthSynced.Add(model);
            model.AddMouthSync(this);
        }

        public void RemoveAllMouthSync(List<Live2DActorEntity> traversalBuffer = null)
        {
            traversalBuffer ??= new List<Live2DActorEntity>();

            traversalBuffer.Add(this);

            foreach (var modelController in mouthSynced.Where(e => !traversalBuffer.Contains(e)))
            {
                modelController.RemoveAllMouthSync(traversalBuffer);
            }

            mouthSynced.RemoveAll(x => true);
        }

        public void SetEye(string motion)
        {
            switch (motion)
            {
                case "閉じ":
                    UseEyeBlink = false;
                    ManualEyeOpen = false;
                    break;
                case "開き":
                    UseEyeBlink = false;
                    ManualEyeOpen = true;
                    break;
                default:
                    UseEyeBlink = true;
                    ManualEyeOpen = false;
                    break;
            }
        }

        public void Render()
        {
            // skip live2d rendering in batch mode
            // if (BatchMode) return;

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GL.Clear(true, true, Color.clear);

            if (isActiveAndEnabled && !Hidden)
            {
                if (live2DModel == null) return;
                if (live2DModel.getRenderMode() == live2d.Live2D.L2D_RENDER_DRAW_MESH_NOW)
                {
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.LoadProjectionMatrix(live2DCanvasPos);

                    // this will generate one and only one error but it is absolutely okay
                    // the cause is that in the draw() call, live2d SDK checks if the stacktrace string
                    // contains "OnPostRender()" while in the extracted stacktrace the function string
                    // is actually "OnPostRender ()" instead
                    // other than an error log, the SDK does absolutely nothing so it's benign
                    // on iOS platform it doesn't generate the error message
                    live2DModel.draw();

                    GL.PopMatrix();
                }
            }
            
            RenderTexture.active = active;
        }

        public static void ReorderLayers()
        {
            if (AllActors == null) return;

            AllActors.Sort((a, b) => a.layer.CompareTo(b.layer));
            for (int i = 0; i < AllActors.Count; i++)
            {
                AllActors[i].transform.SetSiblingIndex(i);
            }
        }

        public IEnumerator ActorAngle(float angleX, float angleY, float duration)
        {
            if (duration == 0f)
            {
                AddAngleX = angleX;
                AddAngleY = angleY;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(AddAngleX, angleX, duration,
                value => { AddAngleX = value; }));
            s.Join(DOVirtual.Float(AddAngleY, angleY, duration,
                value => { AddAngleY = value; }));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorBodyAngle(float angleX, float duration)
        {
            if (duration == 0f)
            {
                AddBodyAngleX = angleX;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(AddBodyAngleX, angleX, duration,
                value => { AddBodyAngleX = value; }));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorEnter(Vector3 originalPos, Vector3 targetPos, float duration)
        {
            Position = targetPos;
            Hidden = false;

            if (duration == 0)
            {
                Position = originalPos;
                yield break;
            }

            Sequence sequence = GetSequence();
            sequence.Append(DOVirtual.Vector3(targetPos, originalPos, duration, value => { Position = value; }));

            yield return sequence.WaitForCompletion();
            RemoveSequence(sequence);
        }

        public IEnumerator ActorExit(Vector3 originalPos, Vector3 targetPos, float duration)
        {
            if (duration == 0)
            {
                Position = targetPos;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Vector3(originalPos, targetPos, duration, value => Position = value));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorEyeAdd(float addAngle, float duration)
        {
            if (duration == 0)
            {
                AddEyeX = addAngle;
                AbsoluteEyeX = 0;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(0, addAngle, duration,
                value =>
                {
                    AddEyeX = value;
                    AbsoluteEyeX = 0;
                }));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorScale(float scale, float duration)
        {
            RectTransform rt = GetComponent<RectTransform>();

            float originalScale = rt.localScale.x;

            if (duration == 0)
            {
                rt.localScale = Vector3.one * scale;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(originalScale, scale, duration,
                value => rt.localScale = Vector3.one * value));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }
    }
}