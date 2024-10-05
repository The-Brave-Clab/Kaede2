
// ReSharper disable IdentifierTypo InconsistentNaming

using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class MasterZukanUdonInfo : BaseMasterData<MasterZukanUdonInfo, ZukanInfo>
    {
        [SerializeField]
        private ZukanInfo[] zukanInfo;
        public override ZukanInfo[] Data => zukanInfo;
    }
}