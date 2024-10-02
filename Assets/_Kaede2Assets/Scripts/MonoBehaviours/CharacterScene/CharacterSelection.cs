using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Kaede2
{
    public class CharacterSelection : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField]
        private SelectionOutlineColor outline;

        [SerializeField]
        private Image image;

        private static CharacterSelection selected;

        private CharacterSceneController sceneController;
        private MasterCharaProfile.CharacterProfile profile;

        private AsyncOperationHandle<Sprite> iconHandle;
        private AsyncOperationHandle<Sprite> standingHandle;

        public IEnumerator Initialize(CharacterSceneController controller, MasterCharaProfile.CharacterProfile p, bool isSelected)
        {
            sceneController = controller;
            profile = p;

            CoroutineGroup group = new();

            iconHandle = ResourceLoader.LoadCharacterIcon(profile.Thumbnail);
            standingHandle = ResourceLoader.LoadCharacterSprite(profile.StandingPic);

            group.Add(iconHandle);
            group.Add(standingHandle);

            yield return group.WaitForAll();

            image.sprite = iconHandle.Result;

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
            sceneController.CharacterPreviewImage.sprite = standingHandle.Result;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            Confirm();
        }

        private void OnDestroy()
        {
            if (iconHandle.IsValid())
            {
                Addressables.Release(iconHandle);
            }

            if (standingHandle.IsValid())
            {
                Addressables.Release(standingHandle);
            }
        }
    }
}
