using Kaede2.UI.Framework;
using UnityEngine;

namespace Kaede2
{
    public class MainMenuSelectableItem : SelectableItem
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string messageWindowText;

        public string MessageWindowText => messageWindowText;
    }
}