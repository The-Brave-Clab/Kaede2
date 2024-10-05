using System;
using Kaede2.Scenario.Framework;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCollabInfo : BaseMasterData<MasterCollabInfo, MasterCollabInfo.CollabInfo>
    {
        public enum CollabType
        {
            [InspectorName("RELEASE THE SPYCE")]
            RELEASE_THE_SPYCE = 1,
            [InspectorName("刀使ノ巫女")]
            TOJI_NO_MIKO = 2,
            [InspectorName("とある科学の超電磁砲T")]
            TO_ARU_KAGAKU_NO_RAILGUN = 3
        }

        [Serializable]
        public class CollabInfo
        {
            public int No;
            public int CollabId;
            public CollabType CollabType;
            public string CollabName;
            public int StoryChapterId;
            public int StoryEpisodeId;
            public int SelfIntroChapterId;
            public int SelfIntroEpisodeId;
            public string BannerImage;
            public string LogoImage;
            public string MainBgImage;
            public string CollabStoryBanner;
            public string CharaVoiceBanner;
            public string SelfIntroBanner;
            public string CharaVoiceBgImage;
            public string TitleLogoImage;
        }

        [SerializeField]
        private CollabInfo[] collabInfo;
        public override CollabInfo[] Data => collabInfo;
    }
}