using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede2
{
    public class ZukanViewWindow : MonoBehaviour, Kaede2InputAction.ICharacterZukanViewActions
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
            InputManager.InputAction.Character.Disable();
            InputManager.InputAction.CharacterZukanView.Enable();
            InputManager.InputAction.CharacterZukanView.AddCallbacks(this);
        }

        private void Hide()
        {
            InputManager.InputAction.CharacterZukanView.RemoveCallbacks(this);
            InputManager.InputAction.CharacterZukanView.Disable();
            InputManager.InputAction.Character.Enable();
            window.gameObject.SetActive(false);
            AudioManager.CancelSound();
        }

        private void Save()
        {
            if (image.sprite == null) return;

            AudioManager.ConfirmSound();
            SaveTexture.Save(window.TitleText, image.sprite.texture);
        }

        public void OnBack(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Hide();
        }

        public void OnSave(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Save();
        }
    }
}