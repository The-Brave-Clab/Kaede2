using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects.MasterData
{
    [Serializable]
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