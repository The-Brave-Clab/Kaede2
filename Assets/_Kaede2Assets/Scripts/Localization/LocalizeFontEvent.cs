using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Kaede2.Localization
{
    [System.Serializable]
    public class LocalizedFont : UnityEngine.Localization.LocalizedAsset<TMP_FontAsset> {}
 
    [System.Serializable]
    public class UpdateFontEvent : UnityEvent<TMP_FontAsset>{}
 
    public class LocalizeFontEvent :  LocalizedAssetEvent<TMP_FontAsset, LocalizedFont, UpdateFontEvent>
    {
    }
}