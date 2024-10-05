using System;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterEventStoryImageData : BaseMasterData<MasterEventStoryImageData, MasterEventStoryImageData.EventStoryImage>
    {
        [Serializable]
        public class EventStoryImage
        {
            public int No;
            public int ScenarioId;
            public int EpisodeId;
            public int KindId;
            public string EpisodeName;
            public string StoryName;
            public string FileName;
        }

        [SerializeField]
        private EventStoryImage[] eventStoryImages;
        public override EventStoryImage[] Data => eventStoryImages;
    }
}