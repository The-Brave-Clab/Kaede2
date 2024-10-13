using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public abstract class SettingsControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent onPointerEnter;

        protected bool activated;

        private bool shouldActivate;
        private static SettingsControl currentPointerDown = null;
        private static SettingsControl currentPointerOver = null;

        private SettingsItem settingsItem;

        protected abstract void OnActivate();
        protected abstract void OnDeactivate();

        protected virtual void Awake()
        {
            activated = false;
            shouldActivate = false;

            // find the SettingsItem component in the parent recursively
            Transform parent = transform.parent;
            while (parent != null)
            {
                settingsItem = parent.GetComponent<SettingsItem>();
                if (settingsItem != null)
                    break;
                parent = parent.parent;
            }
        }

        private void ChangeActivationStatus(bool newStatus)
        {
            if (currentPointerDown != null) return;
            if (newStatus == activated) return;
            activated = newStatus;

            if (newStatus)
                OnActivate();
            else
                OnDeactivate();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            shouldActivate = true;
            currentPointerOver = this;
            ChangeActivationStatus(true);
            onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            shouldActivate = false;
            currentPointerOver = null;
            ChangeActivationStatus(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            currentPointerDown = this;
            if (settingsItem != null)
                settingsItem.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (currentPointerDown == this)
                currentPointerDown = null;
            ChangeActivationStatus(shouldActivate);
            if (currentPointerOver != null)
                currentPointerOver.ChangeActivationStatus(currentPointerOver.shouldActivate);
            if (settingsItem != null)
                settingsItem.OnPointerUp(eventData);
        }

        public abstract void Left();
        public abstract void Right();
    }
}