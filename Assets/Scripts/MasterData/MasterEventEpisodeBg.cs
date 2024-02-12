using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
{
    [Serializable]
    public class MasterEventEpisodeBg : BaseMasterData
    {
        [Serializable]
        public class EventEpisodeBg
        {
            public int No;
            public int KindId;
            public int EpisodeId;
            public string EpisodeBg;
            public int VolumeNumber;
        }

        public EventEpisodeBg[] eventEpisodeBgs;
    }
}