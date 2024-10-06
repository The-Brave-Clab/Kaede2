using System.Collections;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class ArrowButtonWithDecor : MonoBehaviour, IThemeChangeObserver, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField]
        private Image foreground;

        [SerializeField]
        private UnityEvent onClick;

        private RectTransform rt;
        private Vector3 originalScale;

        private Coroutine coroutine;
        private Sequence sequence;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
            rt = GetComponent<RectTransform>();
            originalScale = rt.localScale;
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            foreground.color = theme.ArrowWithDecor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ClearCoroutine();
            coroutine = StartCoroutine(ScaleCoroutine(true));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ClearCoroutine();
            coroutine = StartCoroutine(ScaleCoroutine(false));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }

        private void ClearCoroutine()
        {
            if (coroutine == null) return;
            StopCoroutine(coroutine);
            sequence.Kill();
            coroutine = null;
            sequence = null;
        }

        private IEnumerator ScaleCoroutine(bool pointerDown)
        {
            var startScale = rt.localScale;
            var targetScale = pointerDown ? originalScale * 1.1f : originalScale;

            sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(0, 1, 0.1f, value =>
            {
                rt.localScale = Vector3.Lerp(startScale, targetScale, value);
            }));

            yield return sequence.WaitForCompletion();

            coroutine = null;
            sequence = null;
        }
    }
}