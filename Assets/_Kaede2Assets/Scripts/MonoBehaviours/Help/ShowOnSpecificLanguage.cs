using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kaede2.Localization;
using UnityEngine;

namespace Kaede2
{
    public class ShowOnSpecificLanguage : MonoBehaviour
    {
        [SerializeField]
        private List<SerializableCultureInfo> cultures;

        private void Awake()
        {
            LocalizationManager.OnLocaleChanged += OnLocaleChanged;
            OnLocaleChanged(LocalizationManager.CurrentLocale);
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(CultureInfo info)
        {
            if (cultures == null) return;
            gameObject.SetActive(cultures.Any(culture => culture.Equals(info)));
        }
    }
}