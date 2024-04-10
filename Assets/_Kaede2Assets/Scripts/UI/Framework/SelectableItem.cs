using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kaede2.UI.Framework
{
    public class SelectableItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public bool selected;
        private bool lastSelected;

        public UnityEvent onSelected;
        public UnityEvent onDeselected;
        public UnityEvent onConfirmed;

        protected virtual void Awake()
        {
            lastSelected = false;
        }

        protected virtual void Update()
        {
            if (selected == lastSelected) return;
            lastSelected = selected;

            if (selected)
                onSelected?.Invoke();
            else
                onDeselected?.Invoke();
        }

        public void Confirm()
        {
            if (selected)
                onConfirmed?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onSelected?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Confirm();
        }
    }
}
