using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class AlbumViewController : MonoBehaviour
    {
        [SerializeField]
        private GameObject albumItemPrefab;

        [SerializeField]
        private RectTransform albumItemParent;

        [SerializeField]
        private float unloadAssetInterval = 10;

        [SerializeField]
        private ScrollRect scroll;

        private MasterAlbumInfo masterData;

        private List<AlbumItem> albumItems;

        private float unloadAssetTimer;

        private Func<MasterAlbumInfo.AlbumInfo, bool> currentFilter;

        private void Awake()
        {
            masterData = MasterAlbumInfo.Instance;

            albumItems = new()
            {
                Capacity = masterData.albumInfo.Length
            };

            currentFilter = info => true;

            int order = 0;
            foreach (var album in MasterAlbumInfo.Sorted)
            {
                var albumItem = Instantiate(albumItemPrefab, albumItemParent).GetComponent<AlbumItem>();
                albumItem.gameObject.name = $"{album.AlbumName} [{album.ViewName}]";
                albumItem.transform.SetSiblingIndex(order);
                albumItem.AlbumInfo = album;
                albumItem.Scroll = scroll;
                if (order == 0) albumItem.Select(false);
                albumItems.Add(albumItem);
                ++order;
            }

            unloadAssetTimer = unloadAssetInterval;
        }

        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);
        }

        private void Update()
        {
            unloadAssetTimer -= Time.deltaTime;
            if (unloadAssetTimer <= 0)
            {
                unloadAssetTimer = unloadAssetInterval;
                Resources.UnloadUnusedAssets();
            }
        }

        public void SetFilter(Func<MasterAlbumInfo.AlbumInfo, bool> filter)
        {
            currentFilter = filter ?? (_ => true);

            AlbumItem firstItem = null;
            foreach (var albumItem in albumItems)
            {
                var filterResult = currentFilter(albumItem.AlbumInfo);
                albumItem.gameObject.SetActive(filterResult);

                if (filterResult && firstItem == null)
                    firstItem = albumItem;
            }

            var selected = AlbumItem.CurrentSelected;
            if (selected == null || !currentFilter(selected.AlbumInfo))
                selected = firstItem;

            if (selected == null)
                return;

            // we use a coroutine to ensure that the selected item is visible one frame after it's set to active
            IEnumerator ForceSelectedVisibleCoroutine(AlbumItem item)
            {
                scroll.StopMovement();
                yield return null;
                item.Select(true);
            }

            CoroutineProxy.Start(ForceSelectedVisibleCoroutine(selected));
        }

        private AlbumItem GetFirst()
        {
            return albumItems.FirstOrDefault(item => currentFilter(item.AlbumInfo));
        }

        public AlbumItem GetPrevious()
        {
            var current = AlbumItem.CurrentSelected;
            if (current == null) return GetFirst();

            var index = albumItems.IndexOf(current);
            if (index == -1) return GetFirst();

            if (index == 0) return null;

            for (int i = index - 1; i >= 0; --i)
                if (currentFilter(albumItems[i].AlbumInfo))
                    return albumItems[i];

            return null;
        }

        public AlbumItem GetNext()
        {
            var current = AlbumItem.CurrentSelected;
            if (current == null) return GetFirst();

            var index = albumItems.IndexOf(current);
            if (index == -1) return GetFirst();

            if (index == albumItems.Count - 1) return null;

            for (int i = index + 1; i < albumItems.Count; ++i)
                if (currentFilter(albumItems[i].AlbumInfo))
                    return albumItems[i];

            return null;
        }
    }
}