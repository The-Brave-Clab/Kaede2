using System;
using System.IO;
using SmartAddresser.Editor.Core.Models.Shared.AssetGroups.AssetFilterImpl;
using UnityEditor;
using UnityEngine;

namespace Kaede2.Editor.SmartAddresser
{
    [CreateAssetMenu(fileName = "Kaede2AssetFilter", menuName = "Smart Addresser/Kaede2/Asset Filter")]
    public sealed class Kaede2AssetFilter : AssetFilterAsset
    {
        [SerializeField]
        private UnityEngine.Object assetBase;

        private string assetBasePath = "";

        public string AssetBasePath
        {
            get
            {
                if (string.IsNullOrEmpty(assetBasePath))
                    assetBasePath = AssetDatabase.GetAssetPath(assetBase);
                return assetBasePath;
            }
        }

        private bool CheckIsFolder()
        {
            if (assetBase is not DefaultAsset) return false;
            var assetPath = AssetDatabase.GetAssetPath(assetBase);
            return Directory.Exists(assetPath);
        }

        public override void SetupForMatching()
        {
            if (!CheckIsFolder())
                Debug.LogError("The asset base is not a folder");
        }

        public override bool IsMatch(string assetPath, Type assetType, bool isFolder)
        {
            if (!isFolder) return false;
            if (!assetPath.Contains(AssetBasePath, StringComparison.InvariantCultureIgnoreCase))
                return false;

            var relative = Path.GetRelativePath(AssetBasePath, assetPath);
            relative = relative.Replace('\\', '/');
            relative = relative.Trim('/');

            var segments = relative.Split('/');

            if (segments[0] == "audio" && segments.Length == 2)
                return true;

            if (segments[0] == "scenario" && segments.Length == 2)
                return true;

            if (segments[0] == "scenario_common" && segments.Length > 1)
            {
                if (segments[1] == "live2d" && segments.Length == 3)
                    return true;

                if (segments.Length == 2)
                    return true;
            }

            return false;
        }

        public override string GetDescription()
        {
            return "Asset Filter for Kaede2";
        }
    }
}