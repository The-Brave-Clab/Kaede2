using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterBgmData : BaseMasterData<MasterBgmData, MasterBgmData.BgmData>
    {
        [Serializable]
        public class BgmData
        {
            public int id;
            public string cueName;
            public string bgmTitle;
        }

        [SerializeField]
        private BgmData[] masterBgms;
        public override BgmData[] Data => masterBgms;
    }
}