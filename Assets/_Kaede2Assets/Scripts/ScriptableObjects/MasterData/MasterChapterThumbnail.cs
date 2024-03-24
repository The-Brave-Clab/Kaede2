using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterChapterThumbnail : BaseMasterData<MasterChapterThumbnail>
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