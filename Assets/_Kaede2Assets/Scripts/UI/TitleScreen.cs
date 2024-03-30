using System;
using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField]
        private Image background;

        private void Awake()
        {
            background.sprite = Theme.Vol[GameSettings.ThemeVolume].titleBackground;
        }
    }
}