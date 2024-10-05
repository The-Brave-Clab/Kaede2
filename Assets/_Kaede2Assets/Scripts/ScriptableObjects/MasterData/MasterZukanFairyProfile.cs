
// ReSharper disable IdentifierTypo InconsistentNaming

using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class MasterZukanFairyProfile : BaseMasterData<MasterZukanFairyProfile, ZukanProfile>
    {
        [SerializeField]
        private ZukanProfile[] zukanProfile;
        public override ZukanProfile[] Data => zukanProfile;
    }
}