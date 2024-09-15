using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class StorySelectionItem : MonoBehaviour
    {
        [SerializeField]
        private Image unreadIcon;

        [SerializeField]
        private FavoriteIcon favoriteIcon;

        public bool Unread
        {
            get => unreadIcon.enabled;
            set => unreadIcon.enabled = value;
        }

        public FavoriteIcon FavoriteIcon => favoriteIcon;
    }
}