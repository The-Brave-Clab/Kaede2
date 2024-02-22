using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.Assets.ScriptableObjects
{
    [Serializable]
    public class MasterChapterThumbnail : BaseMasterData
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

        public ChapterThumbnail[] chapterThumb;
    }
}