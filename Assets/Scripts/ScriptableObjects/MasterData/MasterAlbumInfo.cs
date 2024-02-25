using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    [Serializable]
    public class MasterAlbumInfo : BaseMasterData
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