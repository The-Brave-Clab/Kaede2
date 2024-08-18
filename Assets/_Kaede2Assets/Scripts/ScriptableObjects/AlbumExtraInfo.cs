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
            public bool isEvent;
            public bool isBirthday;
            public bool isCollab;
            public bool isNew;
            public string replaceStoryImage;
            public string replaceEpisodeBackground;

            public bool Passes(ImageFilter filter)
            {
                if (filter.HasFlag(ImageFilter.Is16By9) && !is16by9) return false;
                if (filter.HasFlag(ImageFilter.Not16By9) && is16by9) return false;
                if (filter.HasFlag(ImageFilter.Event) && !isEvent) return false;
                if (filter.HasFlag(ImageFilter.NotEvent) && isEvent) return false;
                if (filter.HasFlag(ImageFilter.Birthday) && !isBirthday) return false;
                if (filter.HasFlag(ImageFilter.NotBirthday) && isBirthday) return false;
                if (filter.HasFlag(ImageFilter.Collab) && !isCollab) return false;
                if (filter.HasFlag(ImageFilter.NotCollab) && isCollab) return false;
                if (filter.HasFlag(ImageFilter.New) && !isNew) return false;
                if (filter.HasFlag(ImageFilter.NotNew) && isNew) return false;
                return true;
            }
        }

        [Flags]
        public enum ImageFilter
        {
            Is16By9 = 1 << 0,
            Not16By9 = 1 << 1,
            Event = 1 << 2,
            NotEvent = 1 << 3,
            Birthday = 1 << 4,
            NotBirthday = 1 << 5,
            Collab = 1 << 6,
            NotCollab = 1 << 7,
            New = 1 << 8,
            NotNew = 1 << 9
        }

        public ExtraInfo[] list;
    }
}