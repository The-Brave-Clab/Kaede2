using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterBgmData : BaseMasterData<MasterBgmData>
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