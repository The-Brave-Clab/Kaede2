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
        [TextArea]
        private string text;

        private static InfoBar instance = null;
        public static InfoBar Instance => instance;

        public static string Text
        {
            get => instance == null ? "" : instance.text;
            set
            {
                if (instance != null) instance.text = value;
            }
        }

        public void ChangeText(string newText)
        {
            text = newText;
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;
            instance = this;
        }

        private void Update()
        {
            textComponent.Text = text;
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying) return;
            instance = null;
        }
    }
}