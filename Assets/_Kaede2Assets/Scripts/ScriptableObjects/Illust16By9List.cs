using System;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class Illust16By9List : ScriptableObject
    {
        [Serializable]
        public struct Illust16By9
        {
            public string name;
            public bool is16by9;
        }

        public Illust16By9[] list;
    }
}