using UnityEngine;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class StickWithAnotherRectTransform : MonoBehaviour
    {
        [SerializeField]
        private RectTransform target;
        private RectTransform rt;
        
        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3[] worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);

            Vector2 size = worldCorners[2] - worldCorners[0];

            // Convert the world space position of the source's center to the local space of the target's parent
            Vector3 worldCenter = (worldCorners[0] + worldCorners[2]) / 2;
            Vector3 localCenter = rt.parent == null ? worldCenter : rt.parent.InverseTransformPoint(worldCenter);
            
            var pivot = rt.pivot;
            Vector2 pivotAdjustment = new Vector2(pivot.x - 0.5f, pivot.y - 0.5f);
            pivotAdjustment.x *= size.x;
            pivotAdjustment.y *= size.y;
            
            localCenter.x += pivotAdjustment.x;
            localCenter.y += pivotAdjustment.y;
            //
            // rt.localPosition = localCenter;
            // rt.sizeDelta = size;
            Vector3[] localCorners = new Vector3[4];
            for (int i = 0; i < 4; ++i)
            {
                localCorners[i] = rt.parent.InverseTransformPoint(worldCorners[i]);
            }

            rt.localPosition = localCenter;
            rt.sizeDelta = localCorners[2] - localCorners[0];

            RectTransform parentRT = rt.parent as RectTransform;

            while (parentRT != null && parentRT.parent != null)
            {
                rt.sizeDelta -= parentRT.sizeDelta;
                parentRT = parentRT.parent as RectTransform;
            }
        }
        
    }
}
