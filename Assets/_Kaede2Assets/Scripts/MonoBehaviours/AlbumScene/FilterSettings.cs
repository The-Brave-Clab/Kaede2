using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AudioManager = Kaede2.Audio.AudioManager;

namespace Kaede2
{
    public class FilterSettings : MonoBehaviour, Kaede2InputAction.IAlbumFilterActions
    {
        private static FilterSettings instance;

        [SerializeField]
        private AlbumViewController controller;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GridLayoutGroup[] buttonGroups;

        [SerializeField]
        private CommonButton applyButton;

        private Dictionary<CharacterId, CharacterFilterButton> characterFilterButtons;

        private Dictionary<CharacterId, bool> characterFilter;
        private bool favoriteOnly;

        private int currentButtonGroupIndex; // -1 means apply button
        private int currentButtonIndex;

        public static IReadOnlyDictionary<CharacterId, bool> CharacterFilter => instance == null ? null : instance.characterFilter;
        public static IReadOnlyDictionary<CharacterId, CharacterFilterButton> CharacterFilterButtons => instance == null ? null : instance.characterFilterButtons;

        private void Awake()
        {
            instance = this;

            characterFilterButtons = Enum.GetValues(typeof(CharacterId))
                .Cast<CharacterId>()
                .ToDictionary(id => id, _ => (CharacterFilterButton)null);
            characterFilter = Enum.GetValues(typeof(CharacterId))
                .Cast<CharacterId>()
                .ToDictionary(id => id, id => id == CharacterId.Unknown);
            favoriteOnly = false;

            for (var j = 0; j < buttonGroups.Length; j++)
            {
                var groupIndex = j;
                var group = buttonGroups[j];
                for (int i = 0; i < group.transform.childCount; ++i)
                {
                    var buttonIndex = i;
                    var button = group.transform.GetChild(i).GetComponent<CommonButton>();
                    button.onActivate.AddListener(() =>
                    {
                        currentButtonGroupIndex = groupIndex;
                        currentButtonIndex = buttonIndex;
                    });
                }
            }

            applyButton.onActivate.AddListener(() =>
            {
                currentButtonGroupIndex = -1;
                currentButtonIndex = 0;
            });
        }

        private IEnumerator Start()
        {
            yield return null;
            // delay a frame to ensure the layout is updated
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            instance = null;
        }

        private void OnEnable()
        {
            currentButtonGroupIndex = 0;
            currentButtonIndex = 0;

            InputManager.InputAction.Album.Disable();
            InputManager.InputAction.AlbumFilter.SetCallbacks(this);
            InputManager.InputAction.AlbumFilter.Enable();

            if (InputManager.CurrentDeviceType == InputDeviceType.Touchscreen ||
                InputManager.CurrentDeviceType == InputDeviceType.KeyboardAndMouse)
                return;
    
            buttonGroups[0].GetChildFromLocation(Vector2Int.zero).GetComponent<CommonButton>().OnPointerEnter(null);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.AlbumFilter.Disable();
            InputManager.InputAction.AlbumFilter.RemoveCallbacks(this);
            InputManager.InputAction.Album.Enable();
        }

        public static void RegisterCharacterFilterButton(CharacterId id, CharacterFilterButton button)
        {
            if (instance == null) return;

            instance.characterFilterButtons[id] = button;
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void ApplyFilter()
        {
            controller.SetFilter(Filter);
            gameObject.SetActive(false);
            AudioManager.ConfirmSound();
        }

        private bool Filter(MasterAlbumInfo.AlbumInfo info)
        {
            bool result = true;

            if (favoriteOnly)
                result = result && SaveData.FavoriteAlbumNames
                    .Any(n => n == info.AlbumName);

            result = result && characterFilter
                .Where(pair => pair.Key != CharacterId.Unknown && pair.Value)
                .Select(pair => pair.Key)
                .All(info.CastCharaIds.Contains);

            return result;
        }

        public static void SetCharacterFilter(CharacterId id, bool filter)
        {
            if (instance == null) return;

            instance.characterFilter[id] = filter;

            if (id == CharacterId.Unknown)
                instance.ResetCharacterFilter();
            else
            {
                instance.characterFilter[CharacterId.Unknown] = false;
                instance.characterFilterButtons[CharacterId.Unknown].Deactivate();
            }
        }

        private void ResetCharacterFilter()
        {
            foreach (var id in Enum.GetValues(typeof(CharacterId)).Cast<CharacterId>())
            {
                characterFilter[id] = id == CharacterId.Unknown;
                if (id != CharacterId.Unknown && characterFilterButtons[id] != null)
                    characterFilterButtons[id].Deactivate();
            }
        }

        public void SetFavoriteOnly(bool only)
        {
            favoriteOnly = only;
            AudioManager.ButtonSound();
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentButtonGroupIndex < 0)
            {
                applyButton.OnPointerExit(null);

                currentButtonGroupIndex = buttonGroups.Length - 1;
                var maxLocation = buttonGroups[currentButtonGroupIndex].GetMaxColumnRowCount();
                var item = buttonGroups[currentButtonGroupIndex].GetChildFromLocation(maxLocation);
    
                item.GetComponent<CommonButton>().OnPointerEnter(null);
            }
            else
            {
                var currentItem = buttonGroups[currentButtonGroupIndex].transform.GetChild(currentButtonIndex);
                var currentLocation = buttonGroups[currentButtonGroupIndex].GetLocationFromChild(currentItem);
                var nextLocation = new Vector2Int(currentLocation.x, currentLocation.y - 1);

                if (nextLocation.y < 0)
                {
                    currentButtonGroupIndex -= 1;
                    if (currentButtonGroupIndex < 0) currentButtonGroupIndex = 0;
                    var nextMaxLocation = buttonGroups[currentButtonGroupIndex].GetMaxColumnRowCount();
                    nextLocation.y = nextMaxLocation.y - 1;
                }

                var nextItem = buttonGroups[currentButtonGroupIndex].GetChildFromLocation(nextLocation);

                currentItem.GetComponent<CommonButton>().OnPointerExit(null);
                nextItem.GetComponent<CommonButton>().OnPointerEnter(null);

                scrollRect.MoveItemIntoViewport(nextItem as RectTransform, 0.3f);
            }

            AudioManager.ButtonSound();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentButtonGroupIndex < 0) return;

            var currentItem = buttonGroups[currentButtonGroupIndex].transform.GetChild(currentButtonIndex);
            var currentLocation = buttonGroups[currentButtonGroupIndex].GetLocationFromChild(currentItem);
            var currentMaxLocation = buttonGroups[currentButtonGroupIndex].GetMaxColumnRowCount();
            var nextLocation = new Vector2Int(currentLocation.x, currentLocation.y + 1);

            if (nextLocation.y >= currentMaxLocation.y || nextLocation.y * currentMaxLocation.x + currentLocation.x >= buttonGroups[currentButtonGroupIndex].transform.childCount)
            {
                currentButtonGroupIndex += 1;
                if (currentButtonGroupIndex >= buttonGroups.Length) currentButtonGroupIndex = -1;

                nextLocation.y = 0;
            }

            currentItem.GetComponent<CommonButton>().OnPointerExit(null);

            if (currentButtonGroupIndex == -1)
            {
                applyButton.OnPointerEnter(null);
            }
            else
            {
                var nextItem = buttonGroups[currentButtonGroupIndex].GetChildFromLocation(nextLocation);
                nextItem.GetComponent<CommonButton>().OnPointerEnter(null);

                scrollRect.MoveItemIntoViewport(nextItem as RectTransform, 0.3f);
            }
            
            AudioManager.ButtonSound();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentButtonGroupIndex < 0) return;

            var currentItem = buttonGroups[currentButtonGroupIndex].transform.GetChild(currentButtonIndex);
            var currentLocation = buttonGroups[currentButtonGroupIndex].GetLocationFromChild(currentItem);
            var nextLocation = new Vector2Int(currentLocation.x - 1, currentLocation.y);

            if (nextLocation.x < 0)
            {
                nextLocation.x = buttonGroups[currentButtonGroupIndex].GetMaxColumnRowCount().x - 1;
            }

            var nextItem = buttonGroups[currentButtonGroupIndex].GetChildFromLocation(nextLocation);

            currentItem.GetComponent<CommonButton>().OnPointerExit(null);
            nextItem.GetComponent<CommonButton>().OnPointerEnter(null);
            AudioManager.ButtonSound();
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentButtonGroupIndex < 0) return;

            var currentItem = buttonGroups[currentButtonGroupIndex].transform.GetChild(currentButtonIndex);
            var currentLocation = buttonGroups[currentButtonGroupIndex].GetLocationFromChild(currentItem);
            var currentMaxLocation = buttonGroups[currentButtonGroupIndex].GetMaxColumnRowCount();
            var nextLocation = new Vector2Int(currentLocation.x + 1, currentLocation.y);

            if (nextLocation.x >= currentMaxLocation.x || nextLocation.y * currentMaxLocation.x + nextLocation.x >= buttonGroups[currentButtonGroupIndex].transform.childCount)
            {
                nextLocation.x = 0;
            }

            var nextItem = buttonGroups[currentButtonGroupIndex].GetChildFromLocation(nextLocation);

            currentItem.GetComponent<CommonButton>().OnPointerExit(null);
            nextItem.GetComponent<CommonButton>().OnPointerEnter(null);
            AudioManager.ButtonSound();
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentButtonGroupIndex < 0)
            {
                applyButton.OnPointerClick(null);
            }
            else
            {
                var currentItem = buttonGroups[currentButtonGroupIndex].transform.GetChild(currentButtonIndex);
                currentItem.GetComponent<CommonButton>().OnPointerClick(null);
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            ApplyFilter();

            foreach (var buttonGroup in buttonGroups)
            {
                foreach (Transform button in buttonGroup.transform)
                {
                    button.GetComponent<CommonButton>().OnPointerExit(null);
                }
            }
            applyButton.OnPointerExit(null);
        }
    }
}
