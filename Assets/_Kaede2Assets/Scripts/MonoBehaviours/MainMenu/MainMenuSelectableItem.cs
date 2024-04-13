using System;
using Kaede2.UI.Framework;
using UnityEngine;

namespace Kaede2
{
    public class MainMenuSelectableItem : SelectableItem
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string messageWindowText;

        [NonSerialized]
        public MainMenuController Controller;

        public string MessageWindowText { set => messageWindowText = value; }

        protected override void Awake()
        {
            base.Awake();

            onSelected.AddListener(SetText);
        }

        public void SetText()
        {
            if (Controller == null) return;
            if (Controller.MessageWindow == null) return;
            if (!selected) return;
            Controller.MessageWindow.ChangeText(messageWindowText);
        }
    }
}