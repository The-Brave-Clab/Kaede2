using System;
using UnityEngine;

namespace Kaede2
{
    [Serializable]
    public class SaveData : SavableSingleton<SaveData>
    {
        [SerializeField]
        private int mainMenuBackground = 950040;

        public static int MainMenuBackground
        {
            get => Instance.mainMenuBackground;
            set
            {
                if (value == Instance.mainMenuBackground) return;
                Instance.mainMenuBackground = value;
                Save();
            }
        }
    }
}