using System;
using Kaede2.Scenario.Framework;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCharaProfile : BaseMasterData<MasterCharaProfile>
    {
        [Serializable]
        public class CharacterProfile
        {
            public int No;
            public CharacterId Id;
            public string Name;
            public string Yomi;
            public string CharacterVoice;
            public string Thumbnail;
            public string StandingPic;
            public string Grade;
            public string Height;
            public string Birthday;
            public string BloodType;
            public string Place;
            public string Food;
            public string Work;
            public string Description;
        }

        public CharacterProfile[] charaProfile;
    }
}