using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
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

    [Serializable]
    public class MasterZukanFairyProfile : BaseMasterData
    {
        public ZukanProfile[] zukanProfile;
    }

    [Serializable]
    public class MasterZukanUdonProfile : BaseMasterData
    {
        public ZukanProfile[] zukanProfile;
    }

    [Serializable]
    public class MasterZukanVertexProfile : BaseMasterData
    {
        public ZukanProfile[] zukanProfile;
    }
}