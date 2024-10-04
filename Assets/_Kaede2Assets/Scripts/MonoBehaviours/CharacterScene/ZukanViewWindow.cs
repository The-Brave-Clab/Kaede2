using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class ZukanViewWindow : MonoBehaviour
    {
        [SerializeField] 
        private BoxWindow window;

        [SerializeField]
        private Image image;

        [SerializeField]
        private CommonButton exitButton;

        [SerializeField]
        private CommonButton saveButton;

        private void Awake()
        {
            exitButton.onClick.AddListener(Hide);
            saveButton.onClick.AddListener(Save);
        }

        public void Show(string title, Sprite sprite)
        {
            window.TitleText = title;
            image.sprite = sprite;

            if (sprite == null)
                saveButton.Interactable = false;

            window.gameObject.SetActive(true);
        }

        private void Hide()
        {
            window.gameObject.SetActive(false);
        }

        private void Save()
        {
            if (image.sprite == null) return;

            SaveTexture.Save(window.TitleText, image.sprite.texture);
        }
    }
}