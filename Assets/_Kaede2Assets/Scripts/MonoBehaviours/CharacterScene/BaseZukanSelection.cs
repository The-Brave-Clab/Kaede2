using System.Collections;
using Kaede2.Audio;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public abstract class BaseZukanSelection<T> : CharacterSceneBaseSelection where T : BaseZukanSelection<T>
    {
        [SerializeField]
        private SelectionOutlineColor outline;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Image image;

        private static BaseZukanSelection<T> selected;
        public static BaseZukanSelection<T> Selected => selected;

        protected CharacterSceneController sceneController;
        protected ZukanProfile profile;

        protected AsyncOperationHandle<Sprite> imageHandle;

        public IEnumerator Initialize(CharacterSceneController controller, ZukanProfile p, bool isSelected)
        {
            sceneController = controller;
            profile = p;

            text.text = Text;

            imageHandle = LoadImage();

            yield return imageHandle;

            image.sprite = imageHandle.Result;

            if (isSelected)
            {
                Select();
            }
        }

        public override void Select()
        {
            if (selected != null)
            {
                selected.Deactivate();
            }

            selected = this;
            outline.gameObject.SetActive(true);
            sceneController.ItemSelected();
            SetPreview();
        }

        public override void Confirm()
        {
            if (selected == null) return;

            // if we manually wrap the text, we need to remove it
            sceneController.ZukanViewWindow.Show(Text.Replace("\n", ""), imageHandle.Result);
            AudioManager.ConfirmSound();
        }

        public override void Deactivate()
        {
            outline.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (imageHandle.IsValid())
            {
                Addressables.Release(imageHandle);
            }
        }

        protected abstract AsyncOperationHandle<Sprite> LoadImage();
        protected abstract string Text { get; }
        protected abstract void SetPreview();
    }
}
