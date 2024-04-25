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
            Vector3 scale = Vector3.one;
            Transform parent = transform.parent;
            while (parent != rootCanvasRT)
            {
                scale = Vector3.Scale(scale, parent.localScale);
                parent = parent.parent;
            }
            var newScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
            if (float.IsFinite(newScale.x) && float.IsFinite(newScale.y) && float.IsFinite(newScale.z))
            {
                rectTransform.localScale = newScale;
            }

            rectTransform.rotation = rootCanvasRT.rotation;

            rectTransform.pivot = rootCanvasRT.pivot;
            rectTransform.position = rootCanvasRT.position;
            if (safeArea)
            {
                var safeAreaRect = TransformedSafeArea();
                Vector2 resolution = new Vector2(Screen.width, Screen.height);
                var offset = safeAreaRect.center - resolution / 2.0f;
                var rootScale = rootCanvasRT.localScale;
                rectTransform.position += new Vector3(offset.x, offset.y);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, safeAreaRect.width / rootScale.x);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, safeAreaRect.height / rootScale.y);
            }
            else
            {
                var rootRect = rootCanvasRT.rect;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rootRect.width);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rootRect.height);
            }
        }

        private Rect TransformedSafeArea()
        {
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
