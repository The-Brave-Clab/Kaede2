using System;
using UnityEngine;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class InfoBar : MonoBehaviour
    {
        [SerializeField]
        private TextWithInputButton textComponent;

        [SerializeField]
        [TextArea(3, 10)]
        private string text;

        public string Text
        {
            get => text;
            set => text = value;
        }

        private string lastText;

        private void Update()
        {
            if (text != lastText)
            {
                textComponent.Text = text;
                lastText = text;
            }
        }
    }
}