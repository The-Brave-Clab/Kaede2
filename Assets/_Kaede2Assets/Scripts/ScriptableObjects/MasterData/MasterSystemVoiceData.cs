using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterSystemVoiceData : BaseMasterData<MasterSystemVoiceData, MasterSystemVoiceData.SystemVoiceData>
    {
        [Serializable]
        public class SystemVoiceData
        {
            public int id;
            public VoiceCategory categoryId;
            public string cueName;
        }

        [SerializeField]
        private SystemVoiceData[] masterSystemVoices;
        public override SystemVoiceData[] Data => masterSystemVoices;

        public enum VoiceCategory
        {
            Title = 1,
            CompanyName = 2,
            EnterMainMenu = 3,
            BackToMainMenu = 4,
            Continue = 5,
            Settings = 6,
            SaveSettings = 7,
            MainStory = 8,
            EventStory = 9,
            CharacterProfile = 10,
            Album = 11,
            FavoriteStory = 12,
            BackToTitle = 13,
        }
    }
}