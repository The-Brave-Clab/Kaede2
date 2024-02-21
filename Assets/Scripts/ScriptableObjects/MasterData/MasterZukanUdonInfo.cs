using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects.MasterData
{
    [Serializable]
    public class MasterZukanUdonInfo : BaseMasterData
    {
        public ZukanInfo[] zukanInfo;
    }
}