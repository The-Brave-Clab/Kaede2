using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public abstract class SettingsControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        protected bool activated;

        protected abstract void OnActivate();
        protected abstract void OnDeactivate();

        protected virtual void Awake()
        {
            activated = false;
        }

        private void Activate()
        {
            if (activated) return;
            activated = true;
            OnActivate();
        }

        private void Deactivate()
        {
            if (!activated) return;
            activated = false;
            OnDeactivate();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Activate();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Deactivate();
        }
    }
}