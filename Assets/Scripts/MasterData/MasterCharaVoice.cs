﻿using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
{
    [Serializable]
    public class MasterCharaVoice : BaseMasterData
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

        public CharacterVoice[] charaVoice;
    }
}