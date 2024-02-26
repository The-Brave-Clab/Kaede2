using System;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartAddresser.Editor.Core.Models.LayoutRules.LabelRules;
using UnityEngine;

namespace Kaede2.Editor.SmartAddresser
{
    [CreateAssetMenu(fileName = "Kaede2LabelProvider", menuName = "Smart Addresser/Kaede2/Label Provider")]
    public sealed class Kaede2LabelProvider : LabelProviderAsset
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
            string label = assetBundleFolderRelativePath.Replace('\\', '/').ToLower(CultureInfo.InvariantCulture);

            // for scenario, only keep the first two path segments
            if (label.StartsWith("scenario/"))
            {
                string[] segments = label.Split('/');
                label = string.Join("/", segments.Take(2));
            }

            // for live2d, only keep the first three path segments
            if (label.StartsWith("scenario_common/live2d/"))
            {
                string[] segments = label.Split('/');
                label = string.Join("/", segments.Take(3));
            }

            return $"/{label}";
        }

        public override string GetDescription()
        {
            return "Label Provider for Kaede2";
        }
    }
}