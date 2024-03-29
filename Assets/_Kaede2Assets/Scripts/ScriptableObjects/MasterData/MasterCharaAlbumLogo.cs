﻿using System;
using Kaede2.Scenario.Framework;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCharaAlbumLogo : BaseMasterData<MasterCharaAlbumLogo>
    {
        [Serializable]
        public class CharacterAlbumLogo
        {
            public int No;
            public CharacterId Id;
            public string[] AlbumLogo; // length is 8, one for each volume
        }

        public CharacterAlbumLogo[] charaAlbumLogo;
    }
}