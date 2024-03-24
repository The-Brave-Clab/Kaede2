using System.Linq;
using Kaede2.ScriptableObjects;

namespace Kaede2.Utils
{
    public static class CommonUtils
    {
        public static MasterScenarioInfo.ScenarioInfo GetScenarioInfo(string scenarioName)
        {
            return MasterScenarioInfo.Instance.scenarioInfo.FirstOrDefault(scenarioInfo => scenarioInfo.ScenarioName == scenarioName);
        }

        public static MasterScenarioInfo.ScenarioInfo GetNextScenarioInfo(string scenarioName)
        {
            var currentScenarioInfo = GetScenarioInfo(scenarioName);
            if (currentScenarioInfo == null) return null;

            return MasterScenarioInfo.Instance.scenarioInfo
                .Where(si =>
                    si.KindId == currentScenarioInfo.KindId &&
                    si.ChapterId == currentScenarioInfo.ChapterId &&
                    si.EpisodeId == currentScenarioInfo.EpisodeId)
                .OrderBy(si => si.StoryId)
                .FirstOrDefault(si => si.StoryId > currentScenarioInfo.StoryId);
        }
    }
}