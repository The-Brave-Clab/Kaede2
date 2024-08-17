using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Scenario.Framework;
using Kaede2.Utils;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterAlbumInfo : BaseMasterData<MasterAlbumInfo>
    {
        [Serializable]
        public class AlbumInfo
        {
            public int OriginId;
            public string AlbumName;
            public int Viewtype; // jesus
            public string ViewName;
            public bool IsBg;
            public CharacterId[] CastCharaIds;
        }

        public AlbumInfo[] albumInfo;

        private List<AlbumInfo> sorted;

        public static IReadOnlyList<AlbumInfo> Sorted
        {
            get
            {
                if (Instance.sorted != null && Instance.sorted.Count == Instance.albumInfo.Length) return Instance.sorted;

                Instance.sorted = Instance.albumInfo.OrderBy(ai => ai.OriginId).ThenByDescending(ai => ai.AlbumName).ToList();
                for (int i = 1; i < Instance.sorted.Count - 1; ++i)
                {
                    if (Instance.sorted[i].IsBg && !Instance.sorted[i - 1].IsBg && !Instance.sorted[i + 1].IsBg && Instance.sorted[i].OriginId % 10 == 5)
                    {
                        (Instance.sorted[i], Instance.sorted[i - 1]) = (Instance.sorted[i - 1], Instance.sorted[i]);
                    }
                }

                return Instance.sorted;
            }
        }

        // we have 1999 illustrations, with 40 bundles, each bundle will contain 50 items
        // the split here is caused by main menu scene using one of the illustrations and has a very long loading time
        private const int BundleCount = 40;
        public static int GetBundleIndex(string albumName)
        {
            // find the index inside master data
            int illustIndex = -1;
            int i = 0;
            foreach (var info in Sorted)
            {
                if (string.Equals(info.AlbumName, albumName))
                {
                    illustIndex = i;
                    break;
                }

                ++i;
            }

            if (illustIndex == -1)
            {
                Instance.LogError($"Album {albumName} not found in master data");
                return -1;
            }

            var bundleIndex = illustIndex / Mathf.RoundToInt((float)Instance.albumInfo.Length / BundleCount);
            return bundleIndex;
        }

        public static AlbumInfo FromAlbumName(string albumName)
        {
            return Instance.albumInfo.FirstOrDefault(info => info.AlbumName == albumName);
        }
    }
}