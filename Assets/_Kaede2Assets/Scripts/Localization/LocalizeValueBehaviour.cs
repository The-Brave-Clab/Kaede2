using UnityEngine;

namespace Kaede2.Localization
{
    public class LocalizeValueBehaviour<T> : LocalizeBehaviourBase<T> where T : struct
    {
        [SerializeField]
        private LocalizedValue<T> localizedValue;

        protected override LocalizedItemBase<T> LocalizedItem => localizedValue;
    }
}