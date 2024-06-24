using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI.Framework
{
    public class AdjustGridLayoutCellSize : MonoBehaviour
    {
        [SerializeField]
        private GridLayoutGroup gridLayout;

        [SerializeField]
        private Vector2 targetCellSize;

        private RectTransform rt;

        private void Awake()
        {
            rt = gridLayout.gameObject.GetComponent<RectTransform>();
        }

        void Update()
        {
            var containerWidth = rt.rect.width - gridLayout.padding.left - gridLayout.padding.right;
            var maxConstraintCount = Mathf.FloorToInt(containerWidth / (targetCellSize.x + gridLayout.spacing.x));
            var actualWidth = targetCellSize.x * maxConstraintCount + gridLayout.spacing.x * (maxConstraintCount - 1);
            var widthDiff = containerWidth - actualWidth;
            var cellWidthDiff = widthDiff / maxConstraintCount;
            var cellWidth = targetCellSize.x + cellWidthDiff;
            var cellHeight = cellWidth * targetCellSize.y / targetCellSize.x;
            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}