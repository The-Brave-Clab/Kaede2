using UnityEngine;

namespace Kaede2.Localization
{
    public class LocalizeStringBehaviour : LocalizeBehaviourBase<string>
    {
        [SerializeField]
        private LocalizedString localizedString;

        protected override LocalizedItemBase<string> LocalizedItem => localizedString;
    }
}