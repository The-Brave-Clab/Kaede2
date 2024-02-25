using System;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartAddresser.Editor.Core.Models.LayoutRules.AddressRules;
using UnityEngine;

namespace Kaede2.Editor.SmartAddresserProviders
{
    [CreateAssetMenu(fileName = "Kaede2AddressProvider", menuName = "Smart Addresser/Address Providers/Kaede2 Address Provider")]
    public sealed class Kaede2AddressProvider : AddressProviderAsset
    {
        [SerializeField]
        private string assetsDir = "Assets/AddressableAssets";

        public override void Setup()
        {

        }

        public override string Provide(string assetPath, Type assetType, bool isFolder)
        {
            if (isFolder) return null;

            FileInfo assetFileInfo = new FileInfo(assetPath);

            string assetBundleFolderRelativePath = Path.GetRelativePath(assetsDir, assetFileInfo.FullName);

            if (assetBundleFolderRelativePath.EndsWith(".bytes"))
            {
                assetBundleFolderRelativePath = assetBundleFolderRelativePath[..^6];
            }

            return assetBundleFolderRelativePath.Replace('\\', '/').ToLower(CultureInfo.InvariantCulture);
        }

        public override string GetDescription()
        {
            return "Address Provider for Kaede2";
        }
    }
}