using System;
using UnityEngine;

namespace Kaede2
{
    [ExecuteAlways]
    public class FixedSizeScaler : MonoBehaviour
    {
        [SerializeField]
        private Vector2 targetSize = Vector2.one * 100.0f;

        private DrivenRectTransformTracker tracker;

        private RectTransform rt;
        private RectTransform parentRT;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            parentRT = rt.parent.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            tracker.Add(this, rt, DrivenTransformProperties.All);
        }

        private void OnDisable()
        {
            tracker.Clear();
        }

        private void LateUpdate()
        {
            rt.anchorMax = rt.anchorMin = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = targetSize;

            var parentSize = parentRT.rect.size;
            var aspectRatio = targetSize.x / targetSize.y;
            var parentAspectRatio = parentSize.x / parentSize.y;

            var scale = parentAspectRatio > aspectRatio ? parentSize.y / targetSize.y : parentSize.x / targetSize.x;
            rt.localScale = new Vector3(scale, scale, 1);
        }
    }
}