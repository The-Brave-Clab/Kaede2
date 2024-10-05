using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCartoonInfo : BaseMasterData<MasterCartoonInfo, MasterCartoonInfo.CartoonInfo>
    {
        [Serializable]
        public class CartoonInfo
        {
            public int No;
            public string GroupId;
            public string GroupTitle;
            public string CoverName;
            public string CartoonLabel;
            public string[] ImageNames;
        }

        [SerializeField]
        private CartoonInfo[] cartoonInfo;
        public override CartoonInfo[] Data => cartoonInfo;
    }
}