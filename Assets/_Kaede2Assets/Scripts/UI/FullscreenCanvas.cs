using UnityEngine;

namespace Kaede2.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [ExecuteAlways]
    public class FullscreenCanvas : MonoBehaviour
    {
        protected Canvas canvas;
        protected RectTransform rectTransform;
        protected Canvas rootCanvas;
        protected RectTransform rootCanvasRT;

        protected bool driving;

        [SerializeField]
        private bool safeArea = false;

        [SerializeField]
        private bool ignoreSafeAreaBottom = false;

        private void Awake()
        {
            driving = false;
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponent<Canvas>();

            rootCanvas = canvas.rootCanvas;
            rootCanvasRT = rootCanvas.GetComponent<RectTransform>();

            if (canvas != null && rectTransform != null && rootCanvas != null && rootCanvasRT != null && !canvas.isRootCanvas && rectTransform.drivenByObject == null)
            {
                DrivenRectTransformTracker tracker = new();
                tracker.Clear();
                tracker.Add(this, rectTransform, DrivenTransformProperties.All);
                driving = true;
            }
        }

        protected virtual void LateUpdate()
        {
            if (!driving) return;

            // get accumulated scale and negate it
            Vector3 accumulatedScale = Vector3.one;
            Transform parent = transform.parent;
            while (parent != rootCanvasRT)
            {
                accumulatedScale = Vector3.Scale(accumulatedScale, parent.localScale);
                parent = parent.parent;
            }

            var newScale = new Vector3(1 / accumulatedScale.x, 1 / accumulatedScale.y, 1 / accumulatedScale.z);
            if (float.IsFinite(newScale.x) && float.IsFinite(newScale.y) && float.IsFinite(newScale.z))
            {
                rectTransform.localScale = newScale;
            }

            rectTransform.rotation = rootCanvasRT.rotation;

            rectTransform.pivot = rootCanvasRT.pivot;
            rectTransform.position = rootCanvasRT.position;
            var rootRect = rootCanvasRT.rect;
            Vector2 resolution = new Vector2(Screen.width, Screen.height);
            var rootCanvasScale = new Vector2(rootRect.width / resolution.x, rootRect.height / resolution.y);
            var safeAreaRect = TransformedSafeArea();
            var offset = safeAreaRect.center - resolution / 2.0f;
            var rootScale = rootCanvasRT.localScale;
            rectTransform.position += new Vector3(
                offset.x * rootCanvasScale.x * rootScale.x * accumulatedScale.x,
                offset.y * rootCanvasScale.y * rootScale.y * accumulatedScale.y,
                0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, safeAreaRect.width * rootCanvasScale.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, safeAreaRect.height * rootCanvasScale.y);
        }

        private Rect TransformedSafeArea()
        {
            if (!safeArea)
                return new Rect(0, 0, Screen.width, Screen.height);

            var safeAreaRect = Screen.safeArea;

            if (ignoreSafeAreaBottom)
            {
                var fullRect = new Rect(0, 0, Screen.width, Screen.height);

                if (Screen.orientation == ScreenOrientation.LandscapeLeft)
                {
                    // bottom on the right side
                    safeAreaRect.xMax = fullRect.xMax;
                }
                else if (Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    // bottom on the left side
                    safeAreaRect.xMin = fullRect.xMin;
                }
            }

            return safeAreaRect;
        }
    }
}
