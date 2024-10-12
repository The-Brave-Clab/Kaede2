using System.Collections;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class StardustSelection : CharacterSceneBaseSelection
    {
        [SerializeField]
        private SelectionOutlineColor outline;

        [SerializeField]
        private Image image;

        private static StardustSelection selected;
        public static StardustSelection Selected => selected;

        private CharacterSceneController sceneController;
        private ZukanProfile profile;

        private AsyncOperationHandle<Sprite> imageHandle;

        public IEnumerator Initialize(CharacterSceneController controller, ZukanProfile p, bool isSelected)
        {
            sceneController = controller;
            profile = p;

            imageHandle = ResourceLoader.LoadStardustImage(profile);

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
            sceneController.StardustPreviewImage.sprite = image.sprite;
        }

        public override void Confirm()
        {
            // does nothing
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
    }
}
