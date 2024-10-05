using System;
using UnityEngine;

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

    public class MasterZukanVertexInfo : BaseMasterData<MasterZukanVertexInfo, ZukanInfo>
    {
        [SerializeField]
        private ZukanInfo[] zukanInfo;
        public override ZukanInfo[] Data => zukanInfo;
    }
}