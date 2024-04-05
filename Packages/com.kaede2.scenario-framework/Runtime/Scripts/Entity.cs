using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.Scenario.Framework
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Entity : MonoBehaviour
    {
        public ScenarioModule Module { get; set; }

        public virtual Vector3 Position
        {
            get => UntransformVector(rectTransform.anchoredPosition3D);
            set => rectTransform.anchoredPosition3D = TransformVector(value);
        }

        public Vector2 Pivot
        {
            get => rectTransform.pivot;
            set
            {
                if (rectTransform.pivot == value) return;

                Vector3 anchoredPosition = Position;
                var sizeDelta = rectTransform.sizeDelta;
                var pivot = rectTransform.pivot;
                anchoredPosition.x -= sizeDelta.x * pivot.x;
                anchoredPosition.x += sizeDelta.x * value.x;
                anchoredPosition.y -= sizeDelta.y * pivot.y;
                anchoredPosition.y += sizeDelta.y * value.y;
                rectTransform.pivot = value;
                Position = anchoredPosition;
            }
        }

        protected float ScreenWidthScalar =>
            Module.Fixed16By9 ? 1.0f : Screen.width * 9.0f / 16.0f / Screen.height;

        protected virtual Vector3 TransformVector(Vector3 vec)
        {
            return vec;
        }

        protected virtual Vector3 UntransformVector(Vector3 vec)
        {
            return vec;
        }

        public virtual Color GetColor()
        {
            return Color.clear;
        }

        public virtual void SetColor(Color color)
        {
        }

        private List<Sequence> sequences;
        private Dictionary<string, List<Sequence>> animSequences = null;
        private bool isBeingDestroyed = false;
        private RectTransform rectTransform;

        public bool IsBeingDestroyed => isBeingDestroyed;

        protected virtual void Awake()
        {
            sequences = new List<Sequence>();
            animSequences = new();
            rectTransform = transform as RectTransform;
        }

        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
            foreach (var sequence in sequences)
            {
                sequence.Kill();
            }
        }

        public void Destroy()
        {
            Module.UnregisterEntity(this);
            isBeingDestroyed = true;
            Destroy(gameObject);
        }

        protected Sequence GetSequence()
        {
            Sequence seq = DOTween.Sequence();
            sequences.Add(seq);
            return seq;
        }

        protected void RemoveSequence(Sequence seq)
        {
            sequences.Remove(seq);
        }

        public IEnumerator ColorCommand(Color originalColor, Color targetColor, float duration, Ease ease)
        {
            if (duration == 0)
            {
                SetColor(targetColor);
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Color(originalColor, targetColor, duration, SetColor));
            s.SetEase(ease);

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }


        public IEnumerator MoveAnim(Vector3 originalPosition, Vector3 targetPosition, float duration, bool rebound,
            int loop, Ease ease)
        {
            Sequence seq = GetSequence();
            if (!animSequences.ContainsKey("move"))
            {
                animSequences["move"] = new();
            }

            animSequences["move"].Add(seq);
            seq.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => Position = value));
            if (rebound)
            {
                seq.Append(DOVirtual.Vector3(targetPosition, originalPosition, duration,
                    value => Position = value));
            }

            int loops = (loop < 0) ? loop : (loop + 1);
            seq.SetLoops(loops, LoopType.Restart);
            seq.SetEase(ease);
            seq.OnComplete(() => { animSequences["move"].Remove(seq); });
            seq.OnKill(() =>
            {
                if (isBeingDestroyed) return;

                Sequence s = GetSequence();
                s.Append(DOVirtual.Vector3(Position, originalPosition, duration,
                    value => Position = value));
                s.OnComplete(() => { RemoveSequence(s); });
            });

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public IEnumerator RotateAnim(float originalAngle, float targetAngle, float duration, bool rebound,
            int loop, Ease ease)
        {
            Vector3 eulerAngles = transform.eulerAngles;
            Sequence seq = GetSequence();
            if (!animSequences.ContainsKey("rotate"))
            {
                animSequences["rotate"] = new();
            }

            animSequences["rotate"].Add(seq);
            seq.Append(DOVirtual.Float(originalAngle, targetAngle, duration,
                value =>
                {
                    eulerAngles.z = value;
                    transform.eulerAngles = eulerAngles;
                }));
            if (rebound)
            {
                seq.Append(DOVirtual.Float(targetAngle, originalAngle, duration,
                    value =>
                    {
                        eulerAngles.z = value;
                        transform.eulerAngles = eulerAngles;
                    }));
            }

            int loops = loop < 0 ? loop : loop + 1;
            seq.SetLoops(loops, LoopType.Restart);
            seq.SetEase(ease);
            seq.OnComplete(() => { animSequences["rotate"].Remove(seq); });
            seq.OnKill(delegate
            {
                if (isBeingDestroyed) return;

                Sequence s = GetSequence();
                s.Append(DOVirtual.Float(targetAngle, originalAngle, duration,
                    value =>
                    {
                        eulerAngles.z = value;
                        transform.eulerAngles = eulerAngles;
                    }));
                s.OnComplete(() => { RemoveSequence(s); });
            });

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        public void StopAnim(string animName)
        {
            if (!animSequences.ContainsKey(animName))
            {
                animSequences[animName] = new();
            }

            foreach (var seq in animSequences[animName])
            {
                seq.Kill();
            }

            animSequences[animName].Clear();
        }

        private Sequence moveSequence = null;
        public void EnsureMoveStopped()
        {
            moveSequence?.Kill();
        }

        public IEnumerator Move(Vector3 originalPosition, Vector3 targetPosition, float duration, Ease ease)
        {
            EnsureMoveStopped();

            moveSequence = DOTween.Sequence();
            moveSequence.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => Position = value));
            moveSequence.SetEase(ease);
            moveSequence.onKill = () =>
            {
                Position = targetPosition;
                moveSequence = null;
            };
            moveSequence.onComplete = () => { moveSequence = null; };

            yield return moveSequence.WaitForCompletion();
        }

        private Sequence rotateSequence = null;
        public void EnsureRotateStopped()
        {
            rotateSequence?.Kill();
        }

        public IEnumerator Rotate(float originalAngle, float targetAngle, float duration, Ease ease)
        {
            var eulerAngles = transform.eulerAngles;
            EnsureRotateStopped();

            rotateSequence = DOTween.Sequence();
            rotateSequence.Append(DOVirtual.Float(originalAngle, targetAngle, duration,
                value =>
                {
                    eulerAngles.z = value;
                    transform.eulerAngles = eulerAngles;
                }));
            rotateSequence.SetEase(ease);
            rotateSequence.onKill = () =>
            {
                eulerAngles.z = targetAngle;
                transform.eulerAngles = eulerAngles;
                rotateSequence = null;
            };
            rotateSequence.onComplete = () => { rotateSequence = null; };

            yield return rotateSequence.WaitForCompletion();
        }

        private Sequence scaleSequence = null;
        public void EnsureScaleStopped()
        {
            scaleSequence?.Kill();
        }

        public IEnumerator Scale(Vector3 originalScale, Vector3 targetScale, float duration, Ease ease)
        {
            EnsureScaleStopped();

            scaleSequence = DOTween.Sequence();
            scaleSequence.Append(DOVirtual.Vector3(originalScale, targetScale, duration,
                value => transform.localScale = value));
            scaleSequence.SetEase(ease);
            scaleSequence.onKill = () =>
            {
                transform.localScale = targetScale;
                scaleSequence = null;
            };
            scaleSequence.onComplete = () => { scaleSequence = null; };

            yield return scaleSequence.WaitForCompletion();
        }

        public IEnumerator ColorAlpha(Color color, float fromAlpha, float toAlpha, float duration, bool destroy)
        {
            if (duration <= 0)
            {
                color.a = toAlpha;
                SetColor(color);

                if (destroy)
                    Destroy(gameObject);

                yield break;
            }

            Sequence seq = GetSequence();
            seq.Append(DOVirtual.Float(fromAlpha, toAlpha, duration,
                value =>
                {
                    color.a = value;
                    SetColor(color);
                }));

            if (destroy)
                seq.OnComplete(() => Destroy(gameObject));

            yield return seq.WaitForCompletion();
            RemoveSequence(seq);
        }

        protected EntityTransform GetTransformState()
        {
            var t = transform;

            return new()
            {
                enabled = gameObject.activeSelf,
                position = Position,
                angle = t.eulerAngles.z,
                scale = t.localScale.x,
                pivot = Pivot,
                color = GetColor()
            };
        }

        protected void RestoreTransformState(EntityTransform state)
        {
            var t = transform;

            gameObject.SetActive(state.enabled);
            Position = state.position;
            t.eulerAngles = new Vector3(0, 0, state.angle);
            t.localScale = Vector3.one * state.scale;
            Pivot = state.pivot;
            SetColor(state.color);
        }
    }
}