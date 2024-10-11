using System;
using System.Collections;
using Coffee.UISoftMask;
using DG.Tweening;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class MainMenuController : SelectableGroup, IThemeChangeObserver, IPointerClickHandler, Kaede2InputAction.IMainMenuActions
    {
        [SerializeField]
        private Vector2 size;

        [SerializeField]
        private Image backgroundGradient;

        [SerializeField]
        private Image leftDecor;

        [SerializeField]
        private float menuYStart;

        [SerializeField]
        private float menuYStep;

        [SerializeField]
        private MainMenuMessageWindow messageWindow;

        [SerializeField]
        private SoftMask mask;

        private RectTransform rt;

        private bool driving;

        private Coroutine cursorMoveCoroutine;
        private Sequence cursorMoveSequence;

        public MainMenuMessageWindow MessageWindow => messageWindow;

        protected override void Awake()
        {
            base.Awake();
            driving = false;
            rt = GetComponent<RectTransform>();

            if (rt != null && rt.drivenByObject == null)
            {
                DrivenRectTransformTracker tracker = new();
                tracker.Clear();
                tracker.Add(this, rt, DrivenTransformProperties.SizeDelta);
                driving = true;
            }

            if (!Application.isPlaying) return;

            OnThemeChange(Theme.Current);

            for (int i = 0; i < Items.Count; ++i)
            {
                var item = Items[i] as MainMenuSelectableItem;
                if (item == null) continue;
                var index = i;
                item.Controller = this;
                item.onSelected.AddListener(() => OnSelected(index));
            }

            (SelectedItem as MainMenuSelectableItem)!.SetText();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            InputManager.InputAction.MainMenu.Enable();
            InputManager.InputAction.MainMenu.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.MainMenu.RemoveCallbacks(this);
                InputManager.InputAction.MainMenu.Disable();
            }
        }

        private void Update()
        {
            if (!driving) return;

            float additionalWidth = Screen.orientation == ScreenOrientation.LandscapeLeft
                ? Screen.safeArea.x / rt.lossyScale.x
                : 0; // i'll be damned if any device have safe area on the bottom side of the screen
            var oldSizeDelta = rt.sizeDelta;
            var newSizeDelta = new Vector2(size.x + additionalWidth, size.y);
            if (Vector2.Distance(oldSizeDelta, newSizeDelta) > 0.001f)
            {
                rt.sizeDelta = newSizeDelta;
                StartCoroutine(RefreshMask());
            }
        }

        private IEnumerator RefreshMask()
        {
            mask.enabled = false;
            mask.gameObject.SetActive(false);
            yield return null;
            mask.gameObject.SetActive(true);
            mask.enabled = true;
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            if (!Application.isPlaying) return;
            backgroundGradient.color = theme.MainMenuGradientColor;
            leftDecor.color = theme.MainMenuLeftDecorColor;
        }

        private void OnSelected(int index)
        {
            if (!Application.isPlaying) return;

            if (cursorMoveCoroutine != null)
                StopCoroutine(cursorMoveCoroutine);

            cursorMoveCoroutine = StartCoroutine(MoveCursor(index));
        }

        private IEnumerator MoveCursor(int index)
        {
            if (!Application.isPlaying) yield break;

            var anchoredPos = rt.anchoredPosition;
            var targetY = menuYStart + index * menuYStep;

            cursorMoveSequence?.Kill();

            cursorMoveSequence = DOTween.Sequence();
            cursorMoveSequence.SetEase(Ease.OutQuad);
            cursorMoveSequence.Append(DOVirtual.Float(anchoredPos.y, targetY, 0.2f, value =>
            {
                if (rt == null) return; // in case the object is destroyed
                anchoredPos.y = value;
                rt.anchoredPosition = anchoredPos;
            }));

            yield return cursorMoveSequence.WaitForCompletion();
            cursorMoveSequence = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            Confirm();
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Previous();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Next();
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            CommonUtils.LoadNextScene("TitleScene", LoadSceneMode.Single);
        }
    }
}