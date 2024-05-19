using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.UI
{
    public abstract class TabView : MonoBehaviour
    {
        [SerializeField]
        protected TabGroup tabGroup;

        [SerializeField]
        protected RectTransform content;

        [SerializeField]
        protected RectTransform[] views;

        protected int currentViewIndex;

        protected Coroutine activateViewCoroutine;
        protected Sequence activateViewSequence;

        private void Awake()
        {
            // by default, active the first tab
            currentViewIndex = 0;

            bool first = true;
            foreach (var view in views)
            {
                view.gameObject.SetActive(first);
                first = false;
            }

            content.anchoredPosition = Vector2.zero;
        }

        private void StopCurrent()
        {
            if (activateViewCoroutine == null) return;
            StopCoroutine(activateViewCoroutine);
            activateViewSequence.Kill();
            activateViewCoroutine = null;
            activateViewSequence = null;
        }

        public void ActivateView(int index)
        {

            if (index == currentViewIndex) return;
            StopCurrent();

            activateViewCoroutine = StartCoroutine(ActivateViewCoroutine(index));
        }

        protected abstract IEnumerator ActivateViewCoroutine(int index);
    }
}