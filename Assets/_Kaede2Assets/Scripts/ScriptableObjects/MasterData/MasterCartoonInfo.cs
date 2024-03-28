using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCartoonInfo : BaseMasterData<MasterCartoonInfo>
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

        public CartoonInfo[] cartoonInfo;
    }
}