using System;
using Kaede2.Scenario.Framework;

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
    }
}