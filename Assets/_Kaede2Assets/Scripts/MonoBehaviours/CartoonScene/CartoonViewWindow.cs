using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CartoonViewWindow : MonoBehaviour, Kaede2InputAction.ICartoonViewActions
    {
        [SerializeField]
        private BoxWindow window;

        [SerializeField]
        private ScrollRect scroll;

        [SerializeField]
        private CommonButton backButton;

        [SerializeField]
        private Image frame1;

        [SerializeField]
        private Image frame2;

        [SerializeField]
        private Image frame3;

        [SerializeField]
        private Image frame4;

        private CartoonSceneController sceneController;

        private List<AsyncOperationHandle<Sprite>> handles;

        private Coroutine scrollCoroutine;
        private Sequence scrollSequence;

        private void Awake()
        {
            scrollCoroutine = null;
        }

        private void OnDestroy()
        {
            Clear();
        }

        public IEnumerator Initialize(MasterCartoonInfo.CartoonInfo cartoonInfo, CartoonSceneController controller)
        {
            Clear();

            sceneController = controller;
            scroll.verticalNormalizedPosition = 1;

            var handle1 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[0]);
            var handle2 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[1]);
            var handle3 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[2]);
            var handle4 = ResourceLoader.LoadCartoonFrame(cartoonInfo.ImageNames[3]);

            handles.Add(handle1);
            handles.Add(handle2);
            handles.Add(handle3);
            handles.Add(handle4);

            CoroutineGroup group = new();
            group.Add(handle1);
            group.Add(handle2);
            group.Add(handle3);
            group.Add(handle4);
            yield return group.WaitForAll();

            frame1.sprite = handle1.Result;
            frame2.sprite = handle2.Result;
            frame3.sprite = handle3.Result;
            frame4.sprite = handle4.Result;

            window.TitleText = cartoonInfo.CartoonLabel;
        }

        private void Clear()
        {
            if (handles != null)
            {
                foreach (var handle in handles)
                {
                    if (handle.IsValid())
                        handle.Release();
                }
            }
            handles = new();
        }

        private void ScrollTo(float targetValue)
        {
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
                scrollSequence.Kill();
                scrollCoroutine = null;
                scrollSequence = null;
            }

            scrollCoroutine = StartCoroutine(ScrollToCoroutine(Mathf.Clamp01(targetValue)));
        }

        private IEnumerator ScrollToCoroutine(float targetValue)
        {
            var startValue = scroll.verticalNormalizedPosition;

            scrollSequence = DOTween.Sequence();
            scrollSequence.Append(DOVirtual.Float(0, 1, 0.1f, t =>
            {
                scroll.verticalNormalizedPosition = Mathf.Lerp(startValue, targetValue, t);
            }));

            yield return scrollSequence.WaitForCompletion();

            scrollCoroutine = null;
            scrollSequence = null;
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var currentVerticalValue = scroll.verticalNormalizedPosition;
            var currentIndex = currentVerticalValue * (4 - 1);
            currentIndex = Mathf.Ceil(currentIndex) - currentIndex > 0.1f
                ? Mathf.Ceil(currentIndex)
                : Mathf.Ceil(currentIndex) + 1;
            currentIndex = Mathf.Clamp(currentIndex, 0, 4 - 1);
            ScrollTo(currentIndex / (4 - 1));
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var currentVerticalValue = scroll.verticalNormalizedPosition;
            var currentIndex = currentVerticalValue * (4 - 1);
            currentIndex = currentIndex - Mathf.Floor(currentIndex) > 0.1f
                ? Mathf.Floor(currentIndex)
                : Mathf.Floor(currentIndex) - 1;
            currentIndex = Mathf.Clamp(currentIndex, 0, 4 - 1);
            ScrollTo(currentIndex / (4 - 1));
        }

        public void OnBack(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            sceneController.BackToEpisodeSelection();
        }

        public void OnScroll(InputAction.CallbackContext context)
        {
            // do nothing
        }
    }
}
