using System.Collections;
using DG.Tweening;
using Kaede2.Input;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede2
{
    public class AlbumItemViewCanvas : MonoBehaviour, Kaede2InputAction.IAlbumViewActions
    {
        private static AlbumItemViewCanvas instance;
        public static AlbumItemViewCanvas Instance => instance;

        [SerializeField]
        private AlbumViewController controller;

        [SerializeField]
        private Image background;

        [SerializeField]
        [ColorUsage(false, false)]
        private Color backgroundColor;

        [SerializeField]
        private RectTransform screenViewport;

        [SerializeField]
        private RectTransform mask;

        [SerializeField]
        private AlbumViewItem viewItemPrefab;

        [SerializeField]
        private float spacing = 50;

        [SerializeField]
        private RectTransform topContainer;

        [SerializeField]
        private RectTransform bottomContainer;

        public static bool Enabled
        {
            get => instance != null && instance.gameObject.activeSelf;
            set
            {
                if (instance != null) instance.gameObject.SetActive(value);
            }
        }

        public static Image Background => instance == null ? null : instance.background;
        public static Color BackgroundColor => instance == null ? Color.black : instance.backgroundColor;

        public static RectTransform Viewport => instance == null ? null : instance.screenViewport;
        public static RectTransform Mask => instance == null ? null : instance.mask;

        private AlbumViewItem current;
        private AlbumViewItem previous;
        private AlbumViewItem next;

        private Canvas canvas;

        private bool uiStatus;

        private Coroutine coroutine;
        private Sequence sequence;

        private void Awake()
        {
            instance = this;
            current = null;
            canvas = GetComponent<Canvas>();
            gameObject.SetActive(false);

            uiStatus = true;

            ResetDragStatus();

            InputManager.InputAction.AlbumView.SetCallbacks(this);
            InputManager.InputAction.AlbumView.Save.Disable();
        }

        private void OnDestroy()
        {
            instance = null;

            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.AlbumView.RemoveCallbacks(this);
                InputManager.InputAction.AlbumView.Save.Disable();
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        private void OnEnable()
        {
            UnityEngine.iOS.Device.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.All;
        }

        private void OnDisable()
        {
            UnityEngine.iOS.Device.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.None;
        }
#endif

        public static AlbumViewItem AddItem()
        {
            if (instance == null) return null;
            return Instantiate(instance.viewItemPrefab, instance.mask);
        }

        private void Activate(AlbumViewItem viewing)
        {
            current = viewing;
            var previousItem = controller.GetPrevious();
            var nextItem = controller.GetNext();

            // positioning
            var currentLocalPos = Vector3.zero;

            var viewportSize = screenViewport.rect.size;

            var posDiff = new Vector3(viewportSize.x + spacing, 0, 0);
            var previousLocalPos = -posDiff;
            var nextLocalPos = posDiff;

            current.RectTransform.anchoredPosition = currentLocalPos;
            current.RectTransform.sizeDelta = viewportSize;

            if (previousItem == null)
            {
                if (previous != null)
                    Destroy(previous.gameObject);
            }
            else
            {
                if (previous == null)
                {
                    previous = AddItem().GetComponent<AlbumViewItem>();
                }

                previous.RectTransform.anchoredPosition = previousLocalPos;
                previous.RectTransform.sizeDelta = viewportSize;
                previous.Item = previousItem;
                previous.gameObject.name = previousItem.AlbumInfo.AlbumName;
                previous.Load();
            }

            if (nextItem == null)
            {
                if (next != null)
                    Destroy(next.gameObject);
            }
            else
            {
                if (next == null)
                {
                    next = AddItem().GetComponent<AlbumViewItem>();
                }

                next.RectTransform.anchoredPosition = nextLocalPos;
                next.RectTransform.sizeDelta = viewportSize;
                next.Item = nextItem;
                next.gameObject.name = nextItem.AlbumInfo.AlbumName;
                next.Load();
            }

            current.Item.Select(true);

            InputManager.InputAction.AlbumView.Enable();

            this.Log($"Viewing {current.Item.AlbumInfo.AlbumName}: {current.Item.AlbumInfo.ViewName}");
        }

        private void ClearCoroutine()
        {
            if (coroutine == null) return;

            StopCoroutine(coroutine);
            sequence.Kill();
            coroutine = null;
            sequence = null;
        }

        public static void Enter(AlbumViewItem viewItem)
        {
            if (instance == null) return;
            instance.EnterInternal(viewItem);
        }

        private void EnterInternal(AlbumViewItem viewItem)
        {
            ClearCoroutine();

            gameObject.SetActive(true);

            IEnumerator EnterCoroutine()
            {
                var startCorners = viewItem.Item.WorldCorners;
                var targetCorners = new Vector3[4];
                screenViewport.GetWorldCorners(targetCorners);

                var startWorldPos = (startCorners[0] + startCorners[2]) / 2;
                var targetWorldPos = (targetCorners[0] + targetCorners[2]) / 2;

                var startWorldSize = startCorners[2] - startCorners[0];
                var targetWorldSize = targetCorners[2] - targetCorners[0];

                var startBackgroundColor = background.color;
                var targetBackgroundColor = backgroundColor;
                targetBackgroundColor.a = 1;

                var maskStartWorldPos = (AlbumItem.ViewportWorldCorners[0] + AlbumItem.ViewportWorldCorners[2]) / 2;
                var maskStartLocalPos = screenViewport.InverseTransformPoint(maskStartWorldPos);

                var maskStartWorldTopRight = AlbumItem.ViewportWorldCorners[2];
                var maskStartLocalTopRight = screenViewport.InverseTransformPoint(maskStartWorldTopRight);

                var maskStartSize = (maskStartLocalTopRight - maskStartLocalPos) * 2;
                var maskTargetSize = screenViewport.rect.size;

                var topContainerStartPosY = topContainer.rect.height;
                var topContainerTargetPosY = 0.0f;

                var bottomContainerStartPosY = -bottomContainer.rect.height;
                var bottomContainerTargetPosY = 0.0f;

                sequence = DOTween.Sequence();
                sequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
                {
                    mask.anchoredPosition = Vector2.Lerp(maskStartLocalPos, Vector2.zero, value);
                    mask.sizeDelta = Vector2.Lerp(maskStartSize, maskTargetSize, value);

                    var worldPos = Vector2.Lerp(startWorldPos, targetWorldPos, value);
                    var worldSize = Vector2.Lerp(startWorldSize, targetWorldSize, value);

                    var worldTopRight = worldPos + worldSize / 2;

                    var localPos = mask.InverseTransformPoint(worldPos);
                    var localTopRight = mask.InverseTransformPoint(worldTopRight);
                    var localSize = (localTopRight - localPos) * 2;

                    viewItem.RectTransform.anchoredPosition = localPos;
                    viewItem.RectTransform.sizeDelta = localSize;

                    background.color = Color.Lerp(startBackgroundColor, targetBackgroundColor, value);

                    var topContainerPos = topContainer.anchoredPosition;
                    topContainerPos.y = Mathf.Lerp(topContainerStartPosY, topContainerTargetPosY, value);
                    topContainer.anchoredPosition = topContainerPos;

                    var bottomContainerPos = bottomContainer.anchoredPosition;
                    bottomContainerPos.y = Mathf.Lerp(bottomContainerStartPosY, bottomContainerTargetPosY, value);
                    bottomContainer.anchoredPosition = bottomContainerPos;
                }));
                yield return sequence.WaitForCompletion();
                Activate(viewItem);

                uiStatus = true;

                sequence = null;
                coroutine = null;
            }

            coroutine = StartCoroutine(EnterCoroutine());
        }

        private void Exit()
        {
            ClearCoroutine();

            if (previous != null) Destroy(previous.gameObject);
            if (next != null) Destroy(next.gameObject);

            InputManager.InputAction.AlbumView.Disable();

            IEnumerator ExitCoroutine()
            {
                var startCorners = new Vector3[4];
                current.RectTransform.GetWorldCorners(startCorners);
                var targetCorners = current.Item.WorldCorners;

                var startWorldPos = (startCorners[0] + startCorners[2]) / 2;
                var targetWorldPos = (targetCorners[0] + targetCorners[2]) / 2;

                var startWorldSize = startCorners[2] - startCorners[0];
                var targetWorldSize = targetCorners[2] - targetCorners[0];

                var maskTargetWorldPos = (AlbumItem.ViewportWorldCorners[0] + AlbumItem.ViewportWorldCorners[2]) / 2;
                var maskTargetLocalPos = screenViewport.InverseTransformPoint(maskTargetWorldPos);

                var maskTargetWorldTopRight = AlbumItem.ViewportWorldCorners[2];
                var maskTargetLocalTopRight = screenViewport.InverseTransformPoint(maskTargetWorldTopRight);

                var maskStartSize = Viewport.rect.size;
                var maskTargetSize = (maskTargetLocalTopRight - maskTargetLocalPos) * 2;

                var startBackgroundColor = background.color;
                var targetBackgroundColor = backgroundColor;
                targetBackgroundColor.a = 0;

                var topContainerStartPosY = topContainer.anchoredPosition.y;
                var topContainerTargetPosY = topContainer.rect.height;

                var bottomContainerStartPosY = bottomContainer.anchoredPosition.y;
                var bottomContainerTargetPosY = -bottomContainer.rect.height;

                sequence = DOTween.Sequence();
                sequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
                {
                    mask.anchoredPosition = Vector2.Lerp(Vector2.zero, maskTargetLocalPos, value);
                    mask.sizeDelta = Vector2.Lerp(maskStartSize, maskTargetSize, value);

                    var worldPos = Vector3.Lerp(startWorldPos, targetWorldPos, value);
                    var worldSize = Vector3.Lerp(startWorldSize, targetWorldSize, value);

                    var worldTopRight = worldPos + worldSize / 2;

                    var localPos = mask.InverseTransformPoint(worldPos);
                    var localTopRight = mask.InverseTransformPoint(worldTopRight);
                    var localSize = (localTopRight - localPos) * 2;

                    current.RectTransform.anchoredPosition = localPos;
                    current.RectTransform.sizeDelta = localSize;

                    background.color = Color.Lerp(startBackgroundColor, targetBackgroundColor, value);

                    var topContainerPos = topContainer.anchoredPosition;
                    topContainerPos.y = Mathf.Lerp(topContainerStartPosY, topContainerTargetPosY, value);
                    topContainer.anchoredPosition = topContainerPos;

                    var bottomContainerPos = bottomContainer.anchoredPosition;
                    bottomContainerPos.y = Mathf.Lerp(bottomContainerStartPosY, bottomContainerTargetPosY, value);
                    bottomContainer.anchoredPosition = bottomContainerPos;
                }));

                yield return sequence.WaitForCompletion();

                Destroy(current.gameObject);

                current = null;
                previous = null;
                next = null;

                gameObject.SetActive(false);

                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(ExitCoroutine());
        }

        public void SetNext(bool nextOrPrevious)
        {
            ClearCoroutine();

            InputManager.InputAction.AlbumView.Disable();

            var newCurrentItem = nextOrPrevious ? controller.GetNext() : controller.GetPrevious();
            if (newCurrentItem == null)
            {
                CancelDrag();
                return;
            }

            IEnumerator SetNextCoroutine(bool nextOrPrevious)
            {
                float itemSpacing = screenViewport.rect.width + spacing;

                var currentStartPos = current.RectTransform.anchoredPosition;
                var currentTargetPos = nextOrPrevious ? new Vector2(-itemSpacing, 0) : new Vector2(itemSpacing, 0);

                var previousStartPos =
                    previous == null ? Vector2.zero : previous.RectTransform.anchoredPosition;
                var previousTargetPos = nextOrPrevious ? new Vector2(-2 * itemSpacing, 0) : Vector2.zero;

                var nextStartPos = next == null ? Vector2.zero : next.RectTransform.anchoredPosition;
                var nextTargetPos = nextOrPrevious ? Vector2.zero : new Vector2(2 * itemSpacing, 0);

                sequence = DOTween.Sequence();
                sequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
                {
                    current.RectTransform.anchoredPosition = Vector2.Lerp(currentStartPos, currentTargetPos, value);

                    if (previous != null)
                    {
                        previous.RectTransform.anchoredPosition = Vector2.Lerp(previousStartPos, previousTargetPos, value);
                    }

                    if (next != null)
                    {
                        next.RectTransform.anchoredPosition = Vector2.Lerp(nextStartPos, nextTargetPos, value);
                    }
                }));

                yield return sequence.WaitForCompletion();

                // movement finished, load new previous/next items
                newCurrentItem.Select(true);

                var oldCurrent = current;
                var oldNext = next;
                var oldPrevious = previous;

                current = nextOrPrevious ? oldNext : oldPrevious;
                next = nextOrPrevious ? oldPrevious : oldCurrent;
                previous = nextOrPrevious ? oldCurrent : oldNext;

                Activate(current);

                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(SetNextCoroutine(nextOrPrevious));
        }

        private void CancelDrag()
        {
            ClearCoroutine();

            InputManager.InputAction.AlbumView.Disable();

            IEnumerator CancelDragCoroutine()
            {
                var viewportRect = screenViewport.rect;
                var currentRect = current.RectTransform.rect;

                var viewportSize = viewportRect.size;

                var currentStartPos = current.RectTransform.anchoredPosition;
                var currentTargetPos = currentStartPos;

                const float maxScale = 5.0f;
                var maxSize = new Vector2(viewportSize.x * maxScale, viewportSize.y * maxScale);
                float overSizedScale = -1.0f;

                var currentStartSize = current.RectTransform.rect.size;
                Vector2 currentTargetSize;
                if (currentStartSize.x <= viewportSize.x && currentStartSize.y <= viewportSize.y)
                {
                    currentTargetSize = viewportSize;
                }
                else if (currentStartSize.x > maxSize.x || currentStartSize.y > maxSize.y)
                {
                    currentTargetSize = maxSize;
                    // we only do uniform scaling, so it's safe to calculate from x
                    overSizedScale = currentStartSize.x / maxSize.x;
                }
                else
                {
                    currentTargetSize = currentStartSize;
                }

                if (overSizedScale > 0)
                {
                    var posCenterDiff = currentStartPos - zoomCenter;
                    currentTargetPos = zoomCenter + posCenterDiff / overSizedScale;
                }

                var expectedRect = new Rect(currentTargetPos - currentTargetSize / 2.0f, currentTargetSize);
                // make expectedRect the aspect ratio of content
                Sprite content = current.Image;
                var contentAspectRatio = content.rect.width / content.rect.height;
                if (expectedRect.width / expectedRect.height > contentAspectRatio)
                {
                    var center = expectedRect.center;
                    var width = expectedRect.height * contentAspectRatio;
                    expectedRect = new Rect(center.x - width / 2, expectedRect.yMin, width, expectedRect.height);
                }
                else
                {
                    var center = expectedRect.center;
                    var height = expectedRect.width / contentAspectRatio;
                    expectedRect = new Rect(expectedRect.xMin, center.y - height / 2, expectedRect.width, height);
                }

                var topEdgeDiff = expectedRect.yMax - viewportRect.yMax;
                if (topEdgeDiff < 0)
                {
                    currentTargetPos.y -= topEdgeDiff;
                }

                var bottomEdgeDiff = expectedRect.yMin - viewportRect.yMin;
                if (bottomEdgeDiff > 0)
                {
                    currentTargetPos.y -= bottomEdgeDiff;
                }

                if (expectedRect.height < viewportRect.height)
                    currentTargetPos.y = 0;

                var rightEdgeDiff = expectedRect.xMax - viewportRect.xMax;
                if (rightEdgeDiff < 0)
                {
                    currentTargetPos.x -= rightEdgeDiff;
                }

                var leftEdgeDiff = expectedRect.xMin - viewportRect.xMin;
                if (leftEdgeDiff > 0)
                {
                    currentTargetPos.x -= leftEdgeDiff;
                }

                if (expectedRect.width < viewportRect.width)
                    currentTargetPos.x = 0;

                var posDiff = new Vector2(viewportSize.x + spacing, 0);

                var previousCurrentPos =
                    previous == null ? Vector2.zero : previous.RectTransform.anchoredPosition;
                var previousTargetPos = previous == null ? Vector2.zero : -posDiff;

                var previousCurrentSize = previous == null ? Vector2.zero : previous.RectTransform.rect.size;
                var previousTargetSize = previous == null ? Vector2.zero : viewportSize;

                var nextCurrentPos = next == null ? Vector2.zero : next.RectTransform.anchoredPosition;
                var nextTargetPos = next == null ? Vector2.zero : posDiff;

                var nextCurrentSize = next == null ? Vector2.zero : next.RectTransform.rect.size;
                var nextTargetSize = next == null ? Vector2.zero : viewportSize;

                var startBackgroundColor = background.color;
                var targetBackgroundColor = backgroundColor;
                targetBackgroundColor.a = 1;

                var topContainerStartPosY = topContainer.anchoredPosition.y;
                var topContainerTargetPosY = uiStatus ? 0.0f : topContainer.rect.height;

                var bottomContainerStartPosY = bottomContainer.anchoredPosition.y;
                var bottomContainerTargetPosY = uiStatus ? 0.0f : -bottomContainer.rect.height;

                sequence = DOTween.Sequence();
                sequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
                {
                    current.RectTransform.anchoredPosition = Vector2.Lerp(currentStartPos, currentTargetPos, value);
                    current.RectTransform.sizeDelta = Vector2.Lerp(currentStartSize, currentTargetSize, value);

                    if (previous != null)
                    {
                        previous.RectTransform.anchoredPosition = Vector2.Lerp(previousCurrentPos, previousTargetPos, value);
                        previous.RectTransform.sizeDelta = Vector2.Lerp(previousCurrentSize, previousTargetSize, value);
                    }

                    if (next != null)
                    {
                        next.RectTransform.anchoredPosition = Vector2.Lerp(nextCurrentPos, nextTargetPos, value);
                        next.RectTransform.sizeDelta = Vector2.Lerp(nextCurrentSize, nextTargetSize, value);
                    }

                    background.color = Color.Lerp(startBackgroundColor, targetBackgroundColor, value);

                    var topContainerPos = topContainer.anchoredPosition;
                    topContainerPos.y = Mathf.Lerp(topContainerStartPosY, topContainerTargetPosY, value);
                    topContainer.anchoredPosition = topContainerPos;

                    var bottomContainerPos = bottomContainer.anchoredPosition;
                    bottomContainerPos.y = Mathf.Lerp(bottomContainerStartPosY, bottomContainerTargetPosY, value);
                    bottomContainer.anchoredPosition = bottomContainerPos;
                }));

                yield return sequence.WaitForCompletion();

                InputManager.InputAction.AlbumView.Enable();

                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(CancelDragCoroutine());
        }

        private enum PointerInputType
        {
            Undetermined,
            Horizontal,
            Vertical,
            Zooming,
            Zoomed
        }

        private class PointerInputStatus
        {
            private readonly AlbumItemViewCanvas canvas;

            private InputAction action;

            private bool started;
            private Vector2 current;
            private Vector2 start;
            private Vector2 recordedLast;
            private Vector2 delta;
            private Vector2 totalDelta;
            private bool isSwipe;

            private Vector2 last;

            public bool Started => started;

            public Vector2 Current => current;
            public Vector2 Start => start;
            public Vector2 Last => recordedLast;
            public Vector2 Delta => delta;
            public Vector2 TotalDelta => totalDelta;

            public InputAction Action => action;

            public bool Valid => action != null;
            public bool IsSwipe => isSwipe;

            public PointerInputStatus(AlbumItemViewCanvas canvas)
            {
                this.canvas = canvas;
                SetAction(null);
            }

            public void SetAction(InputAction action)
            {
                this.action = action;

                started = false;
                current = Vector2.zero;
                start = Vector2.zero;
                recordedLast = Vector2.zero;
                delta = Vector2.zero;
                totalDelta = Vector2.zero;
                isSwipe = false;

                last = Vector2.zero;
            }

            public void Update()
            {
                current = canvas.GetTransformedPointerPosition(action.ReadValue<Vector2>());

                if (!started)
                {
                    started = true;
                    start = current;
                    recordedLast = current;
                    delta = Vector2.zero;
                    totalDelta = Vector2.zero;
                    isSwipe = false;
        
                    last = current;
                }
                else
                {
                    delta = current - last;
                    totalDelta = current - start;
                    recordedLast = last;
                    last = current;
                }

                var speed = delta.magnitude / Time.deltaTime;
                // if the speed is high enough and the direction is the same, we consider it a swipe
                isSwipe = speed > 1000 && totalDelta.y * delta.y > 0;
            }
        }

        public bool AllowInteraction { get; set; }
        private PointerInputType pointerInputType;
        private PointerInputStatus primaryPointerStatus;
        private PointerInputStatus secondaryPointerStatus;
        private Vector2 zoomCenter;

        private void ResetDragStatus()
        {
            AllowInteraction = false;
            pointerInputType = PointerInputType.Undetermined;
            primaryPointerStatus = new(this);
            secondaryPointerStatus = new(this);
            zoomCenter = Vector2.zero;
        }

        private Vector2 GetTransformedPointerPosition(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(screenViewport, screenPoint, canvas.worldCamera,
                out var localPoint);
            return localPoint;
        }

        private InputAction GetPointerActionFromPointerContactAction(InputAction.CallbackContext ctx)
        {
            if (ctx.action == InputManager.InputAction.AlbumView.PrimaryPointerContact)
            {
                return InputManager.InputAction.AlbumView.PrimaryPointer;
            }

            if (ctx.action == InputManager.InputAction.AlbumView.SecondaryPointerContact)
            {
                return InputManager.InputAction.AlbumView.SecondaryPointer;
            }

            return null;
        }

        private void StartPrimaryPointerHandler()
        {
            primaryPointerStatus.Update();

            var currentItemScale = current.RectTransform.sizeDelta / screenViewport.rect.size;
            if (currentItemScale.x > 1 || currentItemScale.y > 1)
            {
                pointerInputType = PointerInputType.Zoomed;
            }
            else
            {
                pointerInputType = PointerInputType.Undetermined;
            }
        }

        private void EndPrimaryPointerHandler()
        {
            if (pointerInputType == PointerInputType.Horizontal)
            {
                if (primaryPointerStatus.IsSwipe || Mathf.Abs(primaryPointerStatus.TotalDelta.x) > screenViewport.rect.width / 3)
                    SetNext(primaryPointerStatus.TotalDelta.x < 0); // swiping left goes to the next item
                else
                    CancelDrag();
            }
            else if (pointerInputType == PointerInputType.Vertical)
            {
                if (primaryPointerStatus.IsSwipe || Mathf.Abs(primaryPointerStatus.TotalDelta.y) > screenViewport.rect.height / 3)
                    Exit();
                else
                    CancelDrag();
            }
            else if (pointerInputType is PointerInputType.Zooming or PointerInputType.Zoomed)
            {
                CancelDrag();
            }

            ResetDragStatus();
        }

        private void StartSecondaryPointerHandler()
        {
            secondaryPointerStatus.Update();

            // the secondary pointer should be ignored when input type is already determined
            if (pointerInputType is PointerInputType.Undetermined or PointerInputType.Zoomed)
            {
                pointerInputType = PointerInputType.Zooming;
            }
        }

        private void EndSecondaryPointerHandler()
        {
            if (pointerInputType == PointerInputType.Zooming)
            {
                var currentItemScale = current.RectTransform.sizeDelta / screenViewport.rect.size;
                if (currentItemScale.x > 1 || currentItemScale.y > 1)
                {
                    pointerInputType = PointerInputType.Zoomed;
                }
                else
                {
                    pointerInputType = PointerInputType.Undetermined;
                    CancelDrag();
                }
            }
        }

        public void ToggleUI()
        {
            ClearCoroutine();

            uiStatus = !uiStatus;
            if (uiStatus)
                InputManager.InputAction.AlbumView.Save.Enable();
            else
                InputManager.InputAction.AlbumView.Save.Disable();

            IEnumerator ToggleUICoroutine()
            {
                var topContainerStartPosY = topContainer.anchoredPosition.y;
                var topContainerTargetPosY = uiStatus ? 0.0f : topContainer.rect.height;

                var bottomContainerStartPosY = bottomContainer.anchoredPosition.y;
                var bottomContainerTargetPosY = uiStatus ? 0.0f : -bottomContainer.rect.height;

                sequence = DOTween.Sequence();
                sequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
                {
                    var topContainerPos = topContainer.anchoredPosition;
                    topContainerPos.y = Mathf.Lerp(topContainerStartPosY, topContainerTargetPosY, value);
                    topContainer.anchoredPosition = topContainerPos;

                    var bottomContainerPos = bottomContainer.anchoredPosition;
                    bottomContainerPos.y = Mathf.Lerp(bottomContainerStartPosY, bottomContainerTargetPosY, value);
                    bottomContainer.anchoredPosition = bottomContainerPos;
                }));

                yield return sequence.WaitForCompletion();

                sequence = null;
                coroutine = null;
            }

            coroutine = StartCoroutine(ToggleUICoroutine());
        }

        private void Update()
        {
            bool shouldUpdate = false;

            if (primaryPointerStatus.Started)
            {
                primaryPointerStatus.Update();
                shouldUpdate = true;
            }

            if (secondaryPointerStatus.Started)
            {
                secondaryPointerStatus.Update();
                shouldUpdate = true;
            }

            if (!shouldUpdate) return;

            if (pointerInputType == PointerInputType.Undetermined && primaryPointerStatus.Delta.magnitude > 0.1f)
            {
                pointerInputType = Mathf.Abs(primaryPointerStatus.Delta.x) > Mathf.Abs(primaryPointerStatus.Delta.y)
                    ? PointerInputType.Horizontal
                    : PointerInputType.Vertical;
            }

            if (pointerInputType == PointerInputType.Horizontal)
            {
                var posDiff = screenViewport.rect.size.x + spacing;

                var thisPos = current.RectTransform.anchoredPosition;
                thisPos.x = primaryPointerStatus.TotalDelta.x;
                current.RectTransform.anchoredPosition = thisPos;

                if (previous != null)
                {
                    var pos = previous.RectTransform.anchoredPosition;
                    pos.x = primaryPointerStatus.TotalDelta.x - posDiff;
                    previous.RectTransform.anchoredPosition = pos;
                }

                if (next != null)
                {
                    var pos = next.RectTransform.anchoredPosition;
                    pos.x = primaryPointerStatus.TotalDelta.x + posDiff;
                    next.RectTransform.anchoredPosition = pos;
                }
            }
            else if (pointerInputType == PointerInputType.Vertical)
            {
                float viewportWidth = screenViewport.rect.width;
                float viewportHeight = screenViewport.rect.height;

                float progress = Mathf.Clamp01(Mathf.Abs(primaryPointerStatus.TotalDelta.y) / viewportHeight * 2);

                float sizeScale = Mathf.Lerp(1.0f, 0.5f, progress);

                current.RectTransform.anchoredPosition = primaryPointerStatus.Current - primaryPointerStatus.Start * sizeScale;

                var width = viewportWidth * sizeScale;
                var height = viewportHeight * sizeScale;

                current.RectTransform.sizeDelta = new Vector2(width, height);
                var color = background.color;
                color.a = 1 - Mathf.Clamp01(Mathf.Pow(Mathf.Abs(progress), 2.2f));
                background.color = color;

                var topContainerPos = topContainer.anchoredPosition;
                topContainerPos.y = Mathf.Lerp(uiStatus ? 0 : topContainer.rect.height, topContainer.rect.height, progress);
                topContainer.anchoredPosition = topContainerPos;

                var bottomContainerPos = bottomContainer.anchoredPosition;
                bottomContainerPos.y = Mathf.Lerp(uiStatus ? 0 : -bottomContainer.rect.height, -bottomContainer.rect.height, progress);
                bottomContainer.anchoredPosition = bottomContainerPos;
            }
            else if (pointerInputType == PointerInputType.Zooming)
            {
                var lastCenter = (primaryPointerStatus.Last + secondaryPointerStatus.Last) / 2;
                zoomCenter = (primaryPointerStatus.Current + secondaryPointerStatus.Current) / 2;

                var lastDistance = (primaryPointerStatus.Last - secondaryPointerStatus.Last).magnitude;
                var currentDistance = (primaryPointerStatus.Current - secondaryPointerStatus.Current).magnitude;

                var distanceScale = currentDistance / lastDistance;

                var thisPos = current.RectTransform.anchoredPosition;
                var posCenterDiff = thisPos - lastCenter;
                current.RectTransform.anchoredPosition = zoomCenter + posCenterDiff * distanceScale;
                current.RectTransform.sizeDelta *= distanceScale;
            }
            else if (pointerInputType == PointerInputType.Zoomed)
            {
                current.RectTransform.anchoredPosition += primaryPointerStatus.Delta;
            }
        }

        public static void SaveCurrent()
        {
            SaveTexture.Save(instance.current.Item.AlbumInfo.ViewName, instance.current.Image.texture);
        }

        private void StartPointerHandler(InputAction.CallbackContext ctx)
        {
            InputAction pointerAction = GetPointerActionFromPointerContactAction(ctx);

            if (primaryPointerStatus.Action == null)
            {
                primaryPointerStatus.SetAction(pointerAction);
            }
            else if (secondaryPointerStatus.Action == null)
            {
                secondaryPointerStatus.SetAction(pointerAction);
            }

            int pointerCount = 0;
            if (primaryPointerStatus.Valid) ++pointerCount;
            if (secondaryPointerStatus.Valid) ++pointerCount;

            if (pointerCount == 1)
            {
                StartPrimaryPointerHandler();
            }
            else if (pointerCount == 2)
            {
                StartSecondaryPointerHandler();
            }
        }

        private void EndPointerHandler(InputAction.CallbackContext ctx)
        {
            var pointerAction = GetPointerActionFromPointerContactAction(ctx);

            int pointerCount = 0;
            if (primaryPointerStatus.Valid) ++pointerCount;
            if (secondaryPointerStatus.Valid) ++pointerCount;

            if (pointerCount == 2)
            {
                EndSecondaryPointerHandler();
                // if we ended the first pointer, we need to treat the second pointer as the first pointer
                if (primaryPointerStatus.Action == pointerAction)
                {
                    primaryPointerStatus = secondaryPointerStatus;
                }

                secondaryPointerStatus = new(this);
            }
            else if (pointerCount == 1)
            {
                EndPrimaryPointerHandler();
                primaryPointerStatus = new(this);
            }
        }

        public void OnPrimaryPointer(InputAction.CallbackContext context) { }

        public void OnSecondaryPointer(InputAction.CallbackContext context) { }

        public void OnPrimaryPointerContact(InputAction.CallbackContext context)
        {
            if (context.started)
                StartPointerHandler(context);
            else if (context.canceled)
                EndPointerHandler(context);
        }

        public void OnSecondaryPointerContact(InputAction.CallbackContext context)
        {
            if (context.started)
                StartPointerHandler(context);
            else if (context.canceled)
                EndPointerHandler(context);
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            SetNext(true);
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            SetNext(false);
        }

        public void OnToggleUI(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            ToggleUI();
        }

        public void OnSave(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            SaveCurrent();
        }

        public void OnBack(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Exit();
        }
    }
}
