using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class SelectionArrow : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private int step;

        public void OnPointerClick(PointerEventData eventData)
        {
            selectionControl.Select(step);
        }
    }
}