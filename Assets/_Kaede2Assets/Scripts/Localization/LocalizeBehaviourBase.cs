using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace Kaede2.Localization
{
    public abstract class LocalizeBehaviourBase<T> : MonoBehaviour
    {
        protected abstract LocalizedItemBase<T> LocalizedItem { get; }

        [SerializeField]
        protected UnityEvent<T> onLocaleChanged;

        protected virtual void Awake()
        {
            LocalizationManager.OnLocaleChanged += OnLocaleChanged;
            OnLocaleChanged(LocalizationManager.CurrentLocale);
        }

        protected virtual void OnDestroy()
        {
            LocalizationManager.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(CultureInfo cultureInfo)
        {
            onLocaleChanged.Invoke(LocalizedItem.Get(cultureInfo));
        }
    }
}