using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Kaede2.UI.Framework
{
    [ExecuteAlways]
    public class SelectableItem : MonoBehaviour
    {
        public bool selected;
        private bool lastSelected;

        public UnityEvent<bool> onSelectedChanged;
        public UnityEvent onConfirmed;

        protected virtual void Awake()
        {
            lastSelected = false;
        }

        protected virtual void Update()
        {
            if (selected == lastSelected) return;
            lastSelected = selected;

            onSelectedChanged?.Invoke(selected);
        }

        public void Confirm()
        {
            if (selected)
                onConfirmed?.Invoke();
        }
    }
}
