using System;
using System.Collections;
using UnityEngine;

namespace Kaede2.UI
{
    public abstract class RandomizedImageProvider : MonoBehaviour
    {
        public struct ImageInfo
        {
            public string Name;
            public Sprite Sprite;
        }

        public abstract Vector2 ImageSize { get; }
        public abstract IEnumerator Provide(int count, Action<ImageInfo[]> onProvided);
    }
}