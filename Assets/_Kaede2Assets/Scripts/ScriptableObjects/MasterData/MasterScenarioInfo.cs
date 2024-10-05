using System;
using System.Linq;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterScenarioInfo : BaseMasterData<MasterScenarioInfo, MasterScenarioInfo.ScenarioInfo>
    {
        public enum Kind
        {
            Main = 1,
            Sub = 2,
            Event = 3,
            Birthday = 4,
            Collab = 5,
        }

        [Serializable]
        public class ScenarioInfo
        {
            public int No;
            public int Id;
            public Kind KindId;
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

        [SerializeField]
        private ScenarioInfo[] scenarioInfo;
        public override ScenarioInfo[] Data => scenarioInfo;

        public static ScenarioInfo GetScenarioInfo(string scenarioName)
        {
            return Instance.scenarioInfo.FirstOrDefault(scenarioInfo => scenarioInfo.ScenarioName == scenarioName);
        }

        public static ScenarioInfo GetNextScenarioInfo(string scenarioName)
        {
            var currentScenarioInfo = GetScenarioInfo(scenarioName);
            if (currentScenarioInfo == null) return null;

            return Instance.scenarioInfo
                .Where(si =>
                    si.KindId == currentScenarioInfo.KindId &&
                    si.ChapterId == currentScenarioInfo.ChapterId &&
                    si.EpisodeId == currentScenarioInfo.EpisodeId)
                .OrderBy(si => si.StoryId)
                .FirstOrDefault(si => si.StoryId > currentScenarioInfo.StoryId);
        }
    }
}