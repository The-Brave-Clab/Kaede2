using Kaede2.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kaede2.UI.Framework
{
    public class SelectableItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField]
        private bool useTouchClickProtection = true;

        public bool selected;
        private bool lastSelected;

        public UnityEvent onSelected;
        public UnityEvent onDeselected;
        public UnityEvent onConfirmed;

        private float lastSelectedTime;

        protected virtual void Awake()
        {
            lastSelected = false;
            lastSelectedTime = 0;
        }

        protected virtual void Update()
        {
            if (selected == lastSelected) return;
            lastSelected = selected;

            if (selected)
            {
                lastSelectedTime = Time.time;
                onSelected?.Invoke();
            }
            else
                onDeselected?.Invoke();
        }

        public void Confirm()
        {
            if (selected)
                onConfirmed?.Invoke();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            onSelected?.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // for touch devices, treat first click as hover
            if (useTouchClickProtection && InputManager.CurrentDeviceType == InputDeviceType.Touchscreen && Time.time - lastSelectedTime < 0.3f)
                return;
            Confirm();
        }
    }
}
