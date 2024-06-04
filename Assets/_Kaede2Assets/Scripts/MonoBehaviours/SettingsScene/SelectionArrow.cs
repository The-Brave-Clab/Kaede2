using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class SelectionArrow : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private int step;

        public void OnPointerClick(PointerEventData eventData)
        {
            selectionControl.Select(step);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            selectionControl.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            selectionControl.OnPointerUp(eventData);
        }
    }
}