using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterEventEpisodeBg : BaseMasterData<MasterEventEpisodeBg, MasterEventEpisodeBg.EventEpisodeBg>
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

        [SerializeField]
        private EventEpisodeBg[] eventEpisodeBgs;
        public override EventEpisodeBg[] Data => eventEpisodeBgs;
    }
}