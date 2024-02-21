using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects.MasterData
{
    [Serializable]
    public class MasterBgmData : BaseMasterData
    {
        [Serializable]
        public class BgmData
        {
            public int id;
            public string cueName;
            public string bgmTitle;
        }

        public BgmData[] masterBgms;
    }
}