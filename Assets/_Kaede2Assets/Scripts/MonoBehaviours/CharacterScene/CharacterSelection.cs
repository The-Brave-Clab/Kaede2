using System.Collections;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
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

        private MasterCharaProfile.CharacterProfile profile;

        private AsyncOperationHandle<Sprite> spriteHandle;

        public IEnumerator Initialize(MasterCharaProfile.CharacterProfile p, bool isSelected)
        {
            profile = p;

            spriteHandle = ResourceLoader.LoadCharacterIcon(profile.Thumbnail);
            yield return spriteHandle;
            image.sprite = spriteHandle.Result;

            outline.gameObject.SetActive(isSelected);
            if (isSelected)
            {
                selected = this;
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
    }
}
