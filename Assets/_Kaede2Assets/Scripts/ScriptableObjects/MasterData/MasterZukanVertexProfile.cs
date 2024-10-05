using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    [Serializable]
    public class ZukanProfile
    {
        public int No;
        public int Id;
        public string Name;
        public string Yomi;
        public string CharacterVoice;
        public string Thumbnail;
        public string StandingPic;
        public string NamePicture;
        public string BigPicture;
        public string Birthday;
        public string BloodType;
        public string Place;
        public string Food;
        public string Work;
        public string Description;
    }

    public class MasterZukanVertexProfile : BaseMasterData<MasterZukanVertexProfile, ZukanProfile>
    {
        [SerializeField]
        private ZukanProfile[] zukanProfile;
        public override ZukanProfile[] Data => zukanProfile;
    }
}