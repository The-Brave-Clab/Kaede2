using System;
using System.Collections;
using System.Linq;
using Kaede2.ScriptableObjects;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainMenuBackground : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        private ResourceLoader.LoadAddressableHandle<Sprite> handle;

        private void Awake()
        {
            var illustInfo = MasterAlbumInfo.Instance.albumInfo.First(i => i.OriginId == GameSettings.MainMenuBackground);
            handle = ResourceLoader.LoadIllustration(illustInfo.AlbumName);
        }

        private IEnumerator Start()
        {
            yield return handle.Send();

            image.sprite = handle.Result;
        }

        private void OnDestroy()
        {
            handle?.Dispose();
        }
    }
}