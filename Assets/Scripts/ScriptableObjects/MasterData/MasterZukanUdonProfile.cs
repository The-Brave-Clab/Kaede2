using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects.MasterData
{
    [Serializable]
    public class MasterZukanUdonProfile : BaseMasterData
    {
        public ZukanProfile[] zukanProfile;
    }
}