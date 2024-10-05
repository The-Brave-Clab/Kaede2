using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterChapterThumbnail : BaseMasterData<MasterChapterThumbnail, MasterChapterThumbnail.ChapterThumbnail>
    {
        [Serializable]
        public class ChapterThumbnail
        {
            public int No;
            public int KindId;
            public int ChapterId;
            public string ThumbnailOn;
            public string ThumbnailOff;
            public string ChpLogo;
            public int VolumeNumber;
        }

        [SerializeField]
        private ChapterThumbnail[] chapterThumb;
        public override ChapterThumbnail[] Data => chapterThumb;
    }
}