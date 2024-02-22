using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.Assets.ScriptableObjects
{
    [Serializable]
    public class MasterZukanUdonProfile : BaseMasterData
    {
        public ZukanProfile[] zukanProfile;
    }
}