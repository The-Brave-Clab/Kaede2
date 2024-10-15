using System;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    [RequireComponent(typeof(TabItem))]
    public class TabChangeInfoBarText : MonoBehaviour
    {
        [SerializeField]
        [TextArea]
        private string text;

        public string Text
        {
            get => text;
            set => text = value;
        }

        private void Start()
        {
            var tabItem = GetComponent<TabItem>();
            tabItem.onConfirmed.AddListener(SetInfoBarText);

            if (tabItem.Active)
                SetInfoBarText();
        }

        private void SetInfoBarText()
        {
            InfoBar.Text = text;
        }
    }
}