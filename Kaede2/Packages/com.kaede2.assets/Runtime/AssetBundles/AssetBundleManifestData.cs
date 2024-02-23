using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.Assets.AssetBundles
{
    [Serializable]
    public struct AssetBundleManifestData
    {
        [Serializable]
        public struct Manifest
        {
            public string name;
            public string hash;
            public uint crc;

            public Hash128 Hash => Hash128.Parse(hash);
        }

        public List<Manifest> manifests;
    }
}