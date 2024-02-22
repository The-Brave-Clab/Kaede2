using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.Assets.ScriptableObjects
{
    [Serializable]
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