using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
{
    [Serializable]
    public class ZukanInfo
    {
        public int No;
        public CharacterId Id;
        public string Name;
        public string ViewName;
        public string Yomi;
        public MasterCharaInfo.CharacterInfo.AppealImage[] AppeaImg; // length is 8, one for each volume
        public int Collabo; // could be boolean?
    }

    [Serializable]
    public class MasterZukanFairyInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }

    [Serializable]
    public class MasterZukanUdonInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }

    [Serializable]
    public class MasterZukanVertexInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }
}