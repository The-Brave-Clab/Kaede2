using TMPro;
using UnityEngine;

namespace Kaede2.Localization
{
    public class LocalizeFontBehaviour : LocalizeBehaviourBase<TMP_FontAsset>
    {
        [SerializeField]
        private LocalizedAsset<TMP_FontAsset> localizedFont;

        protected override LocalizedItemBase<TMP_FontAsset> LocalizedItem => localizedFont;
    }
}