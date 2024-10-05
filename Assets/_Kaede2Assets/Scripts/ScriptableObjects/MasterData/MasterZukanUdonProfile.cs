
// ReSharper disable IdentifierTypo InconsistentNaming

using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class MasterZukanUdonProfile : BaseMasterData<MasterZukanUdonProfile, ZukanProfile>
    {
        [SerializeField]
        private ZukanProfile[] zukanProfile;
        public override ZukanProfile[] Data => zukanProfile;
    }
}