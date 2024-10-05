using System;
using Kaede2.Scenario.Framework;
using UnityEngine;

// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects
{
    public class MasterCharaAlbumLogo : BaseMasterData<MasterCharaAlbumLogo, MasterCharaAlbumLogo.CharacterAlbumLogo>
    {
        [Serializable]
        public class CharacterAlbumLogo
        {
            public int No;
            public CharacterId Id;
            public string[] AlbumLogo; // length is 8, one for each volume
        }

        [SerializeField]
        private CharacterAlbumLogo[] charaAlbumLogo;
        public override CharacterAlbumLogo[] Data => charaAlbumLogo;
    }
}