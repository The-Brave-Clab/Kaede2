using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
{
    [Serializable]
    public class MasterScenarioInfo : BaseMasterData
    {
        [Serializable]
        public class ScenarioInfo
        {
            public int No;
            public int Id;
            public int KindId;
            public string KindName;
            public int ChapterId;
            public string ChapterName;
            public int EpisodeId;
            public string EpisodeName;
            public string EpisodeNumber;
            public int StoryId;
            public string StoryName;
            public string Label;
            public string ScenarioName;
            public int VolumeNumber;
        }

        public ScenarioInfo[] scenarioInfo;
    }
}