using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    [Serializable]
    public class ZukanInfo
    {
        public int No;
        public int Id;
        public string Name;
        public string ViewName;
        public string Yomi;
        public MasterCharaInfo.CharacterInfo.AppealImage[] AppeaImg; // length is 8, one for each volume
        public int Collabo; // could be boolean?
    }

    [Serializable]
    public class MasterZukanVertexInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }
}