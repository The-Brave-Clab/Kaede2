#if !UNITY_WEBGL || UNITY_EDITOR

using System.Linq;
using Kaede2.Audio;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class ClearCacheSettingsItemController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TextMeshProUGUI usedStorageText;

        [SerializeField]
        private GameObject clearCacheConfirmBoxPrefab;

        [SerializeField]
        private RectTransform canvas;

        private void Awake()
        {
            var occupied = Enumerable
                .Range(0, Caching.cacheCount)
                .Select(Caching.GetCacheAt)
                .Sum(c => c.spaceOccupied);
            usedStorageText.text = CommonUtils.BytesToHumanReadable(occupied);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Instantiate(clearCacheConfirmBoxPrefab, canvas);
            AudioManager.ConfirmSound();
        }
    }
}

#endif