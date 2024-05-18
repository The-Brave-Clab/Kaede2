using System;
using UnityEngine;

namespace Kaede2.UI
{
    public class InfoBar : MonoBehaviour
    {
        [SerializeField]
        private TextWithInputButton textComponent;

        public string Text
        {
            get => textComponent.Text;
            set => textComponent.Text = value;
        }
    }
}