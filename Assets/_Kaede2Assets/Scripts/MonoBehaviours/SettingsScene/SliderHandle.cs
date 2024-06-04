using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class SliderHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private SliderControl slider;

        private RectTransform rt;
        private RectTransform sliderRT;

        private Vector2 dragStartPos;
        private float dragStartValue;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            sliderRT = slider.GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRT, eventData.position, eventData.pressEventCamera, out dragStartPos);
            dragStartValue = slider.Value;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRT, eventData.position, eventData.pressEventCamera, out var currentPos);
            Vector2 delta = currentPos - dragStartPos;
            float valueDelta = delta.x / sliderRT.rect.width;
            slider.Value = Mathf.Clamp01(dragStartValue + valueDelta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            slider.OnValueChangeEnd();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (slider != null)
                slider.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (slider != null)
                slider.OnPointerUp(eventData);
        }
    }
}