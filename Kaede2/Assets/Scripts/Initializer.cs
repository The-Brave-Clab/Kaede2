using System.Collections;
using UnityEngine;
using Kaede2.Assets.AssetBundles;

namespace Kaede2
{
    // This class initializes many stuff that will be used in the game
    public class Initializer : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return AssetBundleManifestData.LoadManifest();
        }
    }
}