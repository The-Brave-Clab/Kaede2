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
    public class CharacterSelection : CharacterSceneBaseSelection
    {
        [SerializeField]
        private SelectionOutlineColor outline;

        [SerializeField]
        private Image image;

        private static CharacterSelection selected;
        public static CharacterSelection Selected => selected;

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

        public override void Select()
        {
            if (selected != null)
            {
                selected.Deactivate();
            }

            selected = this;
            outline.gameObject.SetActive(true);
            sceneController.ItemSelected();
            sceneController.CharacterPreviewImage.sprite = standingHandle.Result;
        }

        public override void Deactivate()
        {
            outline.gameObject.SetActive(false);
        }

        public override void Confirm()
        {
            sceneController.CharacterProfileController.Enter(profile);
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
