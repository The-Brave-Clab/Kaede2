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
            public int categoryId;
            public string cueName;
        }

        [SerializeField]
        private SystemVoiceData[] masterSystemVoices;
        public override SystemVoiceData[] Data => masterSystemVoices;
    }
}