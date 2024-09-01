using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterEventEpisodeBg : BaseMasterData<MasterEventEpisodeBg>
    {
        [Serializable]
        public class EventEpisodeBg
        {
            public int No;
            public MasterScenarioInfo.Kind KindId;
            public int EpisodeId;
            public string EpisodeBg;
            public int VolumeNumber;
        }

        public EventEpisodeBg[] eventEpisodeBgs;
    }
}