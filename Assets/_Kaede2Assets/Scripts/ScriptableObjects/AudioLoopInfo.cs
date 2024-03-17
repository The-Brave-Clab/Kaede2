using System;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public class AudioLoopInfo : ScriptableObject
    {
        [Serializable]
        public struct LoopInfo
        {
            public int id;
            public int type;
            public int start;
            public int end;
            public int fraction;
            public int play_count;
        }

        public LoopInfo[] loop_info;
    }
}
