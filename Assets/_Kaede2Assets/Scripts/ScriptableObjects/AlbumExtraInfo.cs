using System;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class AlbumExtraInfo : ScriptableObject
    {
        [Serializable]
        public struct ExtraInfo
        {
            public string name;
            public bool is16by9;
        }

        public ExtraInfo[] list;
    }
}