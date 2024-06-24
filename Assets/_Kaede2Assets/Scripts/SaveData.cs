using System;
using System.Collections.Generic;
using Kaede2.ScriptableObjects;
using UnityEngine;

namespace Kaede2
{
    [Serializable]
    public class SaveData : SavableSingleton<SaveData>
    {
        [SerializeField]
        private string mainMenuBackground = "card_image_950040";

        public static MasterAlbumInfo.AlbumInfo MainMenuBackground
        {
            get => MasterAlbumInfo.FromAlbumName(Instance.mainMenuBackground);
            set
            {
                if (value.AlbumName == Instance.mainMenuBackground) return;
                Instance.mainMenuBackground = value.AlbumName;
                Save();
            }
        }

        [SerializeField]
        private List<string> favoriteAlbums = new(); // AlbumName

        public static IReadOnlyList<string> FavoriteAlbumNames => Instance.favoriteAlbums;

        public static void AddFavoriteAlbum(MasterAlbumInfo.AlbumInfo album)
        {
            if (Instance.favoriteAlbums.Contains(album.AlbumName)) return;
            Instance.favoriteAlbums.Add(album.AlbumName);
            Save();
        }

        public static void RemoveFavoriteAlbum(MasterAlbumInfo.AlbumInfo album)
        {
            if (!Instance.favoriteAlbums.Contains(album.AlbumName)) return;
            Instance.favoriteAlbums.Remove(album.AlbumName);
            Save();
        }
    }
}