using System;
using Kaede2.Scenario.Framework;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCollabCharaInfo : BaseMasterData<MasterCollabCharaInfo>
    {
        [Serializable]
        public class CollabCharaInfo
        {
            public int No;
            public CharacterId Id;
            public int CollabId;
            public MasterCollabInfo.CollabType CollabType;
            public string Name;
            public string CVName;
            public string Thumbnail;
            public string StandingImage;
            public string NameImage;
            public string CVNameImage;
            public string Self_Voice;
            public string A_Word_Voice;
            public string Morning_Voice;
            public string Daytime_Voice;
            public string Night_Voice;
            public string Sleep_Voice;
        }

        public CollabCharaInfo[] collabCharaInfo;
    }
}