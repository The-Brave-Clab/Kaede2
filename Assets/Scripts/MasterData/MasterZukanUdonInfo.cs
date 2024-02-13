using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
{
    [Serializable]
    public class MasterZukanUdonInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }
}