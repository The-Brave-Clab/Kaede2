using System;
using UnityEngine;

namespace Kaede2.UI
{
    public class FillParentRect : MonoBehaviour
    {
        [Flags]
        public enum FillType
        {
            None = 0,
            Width = 1,
            Height = 2
        };

        [SerializeField]
        private FillType fillType = FillType.None;

        private RectTransform rt;
        private RectTransform parentRT;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (rt != null)
                parentRT = rt.parent.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (rt == null || parentRT == null) return;

            if (fillType.HasFlag(FillType.Width))
                rt.sizeDelta = new Vector2(parentRT.rect.width, rt.sizeDelta.y);

            if (fillType.HasFlag(FillType.Height))
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, parentRT.rect.height);
        }
    }
}