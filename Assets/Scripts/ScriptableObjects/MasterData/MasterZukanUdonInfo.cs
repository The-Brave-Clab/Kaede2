using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    [Serializable]
    public class MasterZukanUdonInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }
}