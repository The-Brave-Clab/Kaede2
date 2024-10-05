using System;
using Kaede2.Scenario.Framework;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterScenarioCast : BaseMasterData<MasterScenarioCast, MasterScenarioCast.ScenarioCast>
    {
        [Serializable]
        public class ScenarioCast
        {
            public int No;
            public string ScenarioName;
            public CharacterId[] CastCharaIds;
        }

        [SerializeField]
        private ScenarioCast[] scenarioCast;
        public override ScenarioCast[] Data => scenarioCast;
    }
}