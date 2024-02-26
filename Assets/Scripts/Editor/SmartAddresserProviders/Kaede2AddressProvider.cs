using System;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartAddresser.Editor.Core.Models.LayoutRules.AddressRules;
using UnityEngine;

namespace Kaede2.Editor.SmartAddresser
{
    [CreateAssetMenu(fileName = "Kaede2AddressProvider", menuName = "Smart Addresser/Kaede2/Address Provider")]
    public sealed class Kaede2AddressProvider : AddressProviderAsset
    {
        [SerializeField]
        private Kaede2AssetFilter assetFilter;

        public override void Setup()
        {

        }

        public override string Provide(string assetPath, Type assetType, bool isFolder)
        {
            if (!isFolder) return null;

            DirectoryInfo dirInfo = new DirectoryInfo(assetPath);

            string assetBundleFolderRelativePath = Path.GetRelativePath(assetFilter.AssetBasePath, dirInfo.FullName);
            return assetBundleFolderRelativePath.Replace('\\', '/').ToLower(CultureInfo.InvariantCulture);
        }

        public override string GetDescription()
        {
            return "Address Provider for Kaede2";
        }
    }
}