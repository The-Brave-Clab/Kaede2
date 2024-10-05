
// ReSharper disable IdentifierTypo InconsistentNaming

using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class MasterZukanFairyInfo : BaseMasterData<MasterZukanFairyInfo, ZukanInfo>
    {
        [SerializeField]
        private ZukanInfo[] zukanInfo;
        public override ZukanInfo[] Data => zukanInfo;
    }
}