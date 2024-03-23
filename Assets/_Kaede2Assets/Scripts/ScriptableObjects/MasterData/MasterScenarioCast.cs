using System;
using Kaede2.Scenario.Framework;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterScenarioCast : BaseMasterData
    {
        [Serializable]
        public class ScenarioCast
        {
            public int No;
            public string ScenarioName;
            public CharacterId[] CastCharaIds;
        }

        public ScenarioCast[] scenarioCast;
    }
}