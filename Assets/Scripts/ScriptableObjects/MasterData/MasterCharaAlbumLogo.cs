using System;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.ScriptableObjects.MasterData
{
    [Serializable]
    public class MasterCharaAlbumLogo : BaseMasterData
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