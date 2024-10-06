using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabContentController : MonoBehaviour
    {
        [SerializeField]
        private InterfaceTitle interfaceTitle;

        [SerializeField]
        private Image background;

        public void Initialize(CollabProvider provider)
        {
            interfaceTitle.Text = provider.GetComponent<StoryCategorySelectable>().Text;
            background.sprite = provider.Image;
        }
    }
}