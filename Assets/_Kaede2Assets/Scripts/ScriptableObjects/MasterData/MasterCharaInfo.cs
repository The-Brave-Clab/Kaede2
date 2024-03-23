using System;
using Kaede2.Scenario.Framework;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCharaInfo : BaseMasterData
    {
        [Serializable]
        public class CharacterInfo
        {
            [Serializable]
            public class AppealImage
            {
                public bool Profile;
                public bool Illust;
            }

            public int No;
            public CharacterId Id;
            public string Name;
            public string ViewName;
            public string Yomi;
            public AppealImage[] AppeaImg; // length is 8, one for each volume
            public int Collabo; // could be boolean?
        }

        public CharacterInfo[] charaInfo;
    }
}