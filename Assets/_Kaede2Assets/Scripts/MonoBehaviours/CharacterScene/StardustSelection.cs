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
    public class StardustSelection : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField]
        private SelectionOutlineColor outline;

        [SerializeField]
        private Image image;

        private static StardustSelection selected;

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

        private void Select()
        {
            if (selected != null)
            {
                selected.outline.gameObject.SetActive(false);
            }

            selected = this;
            outline.gameObject.SetActive(true);
            sceneController.StardustPreviewImage.sprite = image.sprite;
        }

        private void Confirm()
        {
            if (selected == null) return;

            // TODO
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
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
