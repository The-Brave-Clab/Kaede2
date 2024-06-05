using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class SelectionControlViewport : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private SelectionControl control;

        [SerializeField]
        private RectTransform contentRT;

        private RectTransform rt;

        private Vector2 beginPoint;
        private Vector2 beginPosition;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            control.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            control.OnPointerUp(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out beginPoint);
            beginPosition = contentRT.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out var currentPoint);
            Vector2 delta = currentPoint - beginPoint;

            var leftMostPos = 0; // * rt.rect.width;
            var rightMostPos = -(control.ItemCount - 1) * rt.rect.width;
            var targetPos = beginPosition.x + delta.x;
            if (targetPos > leftMostPos)
            {
                var diff = targetPos - leftMostPos;
                // make the movement non-linear and slower when the distance is larger
                diff = 4 * Mathf.Pow(diff + 1, 0.5f);
                targetPos = leftMostPos + diff;
            }
            else if (targetPos < rightMostPos)
            {
                var diff = targetPos - rightMostPos;
                diff = 4 * Mathf.Pow(-diff + 1, 0.5f);
                targetPos = rightMostPos - diff;
            }

            contentRT.anchoredPosition = new Vector2(targetPos, contentRT.anchoredPosition.y);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var index = control.CalculateIndexFromPosition();
            control.Select(index - control.SelectedIndex);
        }
    }
}
