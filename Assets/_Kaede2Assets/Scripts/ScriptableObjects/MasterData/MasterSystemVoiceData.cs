using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterSystemVoiceData : BaseMasterData
    {
        [Serializable]
        public class SystemVoiceData
        {
            public int id;
            public int categoryId;
            public string cueName;
        }

        public SystemVoiceData[] masterSystemVoices;
    }
}