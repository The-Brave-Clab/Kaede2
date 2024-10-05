using System;
using Kaede2.Scenario.Framework;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCharaVoice : BaseMasterData<MasterCharaVoice, MasterCharaVoice.CharacterVoice>
    {
        [Serializable]
        public class CharacterVoice
        {
            public int No;
            public CharacterId Id;
            public string Name;
            public string Self_Voice;
            public string A_Word_Voice;
            public string Morning_Voice;
            public string Daytime_Voice;
            public string Night_Voice;
            public string Sleep_Voice;
        }

        [SerializeField]
        private CharacterVoice[] charaVoice;
        public override CharacterVoice[] Data => charaVoice;
    }
}