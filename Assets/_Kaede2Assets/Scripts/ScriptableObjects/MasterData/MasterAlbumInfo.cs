using System;
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

        // we have 1999 illustrations, with 40 bundles, each bundle will contain 50 items
        // the split here is caused by main menu scene using one of the illustrations and has a very long loading time
        private const int BundleCount = 40;
        private AlbumInfo[] sortedAlbumInfo = null;
        public static int GetBundleIndex(string albumName)
        {
            // find the index inside master data
            int illustIndex = -1;
            // we are going to display the illust in order of name, so sorting the bundle helps reducing the loading time and memory usage
            Instance.sortedAlbumInfo ??= Instance.albumInfo.OrderBy(i => i.AlbumName).ToArray();
            for (int i = 0; i < Instance.sortedAlbumInfo.Length; i++)
            {
                if (!string.Equals(Instance.sortedAlbumInfo[i].AlbumName, albumName)) continue;
                illustIndex = i;
                break;
            }

            if (illustIndex == -1)
            {
                Instance.LogError($"Album {albumName} not found in master data");
                return -1;
            }

            var bundleIndex = illustIndex / Mathf.RoundToInt((float)Instance.sortedAlbumInfo.Length / BundleCount);
            return bundleIndex;
        }
    }
}