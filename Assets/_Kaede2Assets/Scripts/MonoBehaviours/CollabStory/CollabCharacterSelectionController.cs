using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede2
{
    public class CollabCharacterSelectionController : MonoBehaviour, Kaede2InputAction.ICollabCharacterSelectionActions
    {
        [SerializeField]
        private CollabStoryController storyController;

        [SerializeField]
        private Image background;

        [SerializeField]
        private SelectableGroup characterGroup;

        [SerializeField]
        private StoryCategorySelectable[] railgunCharacters;

        [SerializeField]
        private StoryCategorySelectable[] tojiCharacters;

        [SerializeField]
        private StoryCategorySelectable[] spyceCharacters;

        private CollabImageProvider provider;

        private List<List<StoryCategorySelectable>> railgunLayout;
        private List<List<StoryCategorySelectable>> tojiLayout;
        private List<List<StoryCategorySelectable>> spyceLayout;

        private List<List<StoryCategorySelectable>> currentLayout;
        private Vector2Int currentSelectedLocation;

        private void Awake()
        {
            railgunLayout = new();
            railgunLayout.Add(railgunCharacters.ToList());
            for (int i = 0; i < railgunCharacters.Length; ++i)
            {
                var row = i;
                railgunCharacters[i].onSelected.AddListener(() => currentSelectedLocation = new Vector2Int(0, row));
            }

            tojiLayout = new();
            tojiLayout.Add(tojiCharacters.ToList());
            for (int i = 0; i < tojiCharacters.Length; ++i)
            {
                var row = i;
                tojiCharacters[i].onSelected.AddListener(() => currentSelectedLocation = new Vector2Int(0, row));
            }

            spyceLayout = new();
            spyceLayout.Add(spyceCharacters.Where(s => Array.IndexOf(spyceCharacters, s) % 2 == 0).ToList());
            spyceLayout.Add(spyceCharacters.Where(s => Array.IndexOf(spyceCharacters, s) % 2 != 0).ToList());
            for (int i = 0; i < spyceCharacters.Length; ++i)
            {
                var row = i / 2;
                var col = i % 2;
                spyceCharacters[i].onSelected.AddListener(() => currentSelectedLocation = new Vector2Int(col, row));
            }

            currentSelectedLocation = Vector2Int.zero;
        }

        public IEnumerator Initialize(CollabImageProvider p)
        {
            IEnumerator WaitForCondition(Func<bool> condition)
            {
                while (!condition())
                    yield return null;
            }

            provider = p;

            StoryCategorySelectable[] characterSelectables = provider.CollabType switch
            {
                MasterCollabInfo.CollabType.RELEASE_THE_SPYCE => spyceCharacters,
                MasterCollabInfo.CollabType.TOJI_NO_MIKO => tojiCharacters,
                MasterCollabInfo.CollabType.TO_ARU_KAGAKU_NO_RAILGUN => railgunCharacters,
                _ => throw new ArgumentOutOfRangeException()
            };

            currentLayout = provider.CollabType switch
            {
                MasterCollabInfo.CollabType.RELEASE_THE_SPYCE => spyceLayout,
                MasterCollabInfo.CollabType.TOJI_NO_MIKO => tojiLayout,
                MasterCollabInfo.CollabType.TO_ARU_KAGAKU_NO_RAILGUN => railgunLayout,
                _ => throw new ArgumentOutOfRangeException()
            };

            CoroutineGroup group = new();

            group.Add(provider.LoadCharacterVoiceBackground(s => background.sprite = s));

            foreach (var selectable in characterSelectables)
            {
                group.Add(WaitForCondition(() => selectable.Loaded));
            }

            yield return group.WaitForAll();

            group = new();

            foreach (var selectable in characterSelectables)
            {
                group.Add(selectable.Refresh());
            }

            yield return group.WaitForAll();

            foreach (var selectable in spyceCharacters)
                selectable.gameObject.SetActive(false);
            foreach (var selectable in tojiCharacters)
                selectable.gameObject.SetActive(false);
            foreach (var selectable in railgunCharacters)
                selectable.gameObject.SetActive(false);

            foreach (var selectable in characterSelectables)
                selectable.gameObject.SetActive(true);

            characterGroup.Select(characterSelectables[0]);
        }

        private void OnEnable()
        {
            InputManager.InputAction.CollabCharacterSelection.Enable();
            InputManager.InputAction.CollabCharacterSelection.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.CollabCharacterSelection.RemoveCallbacks(this);
            InputManager.InputAction.CollabCharacterSelection.Disable();
        }

        private Vector2Int ClampLayoutLocation(Vector2Int location)
        {
            location.x = Mathf.Clamp(location.x, 0, currentLayout.Count - 1);
            location.y = Mathf.Clamp(location.y, 0, currentLayout[location.x].Count - 1);
            return location;
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
    
            if (characterGroup.SelectedIndex == characterGroup.Items.Count - 1) return;
            var newLocation = currentSelectedLocation;
            newLocation.y -= 1;
            newLocation = ClampLayoutLocation(newLocation);
            characterGroup.Select(currentLayout[newLocation.x][newLocation.y]);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (characterGroup.SelectedIndex == characterGroup.Items.Count - 1) return;
            var newLocation = currentSelectedLocation;
            newLocation.y += 1;
            newLocation = ClampLayoutLocation(newLocation);
            characterGroup.Select(currentLayout[newLocation.x][newLocation.y]);
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var newLocation = currentSelectedLocation;

            if (characterGroup.SelectedIndex == characterGroup.Items.Count - 1)
            {
                characterGroup.Select(currentLayout[newLocation.x][newLocation.y]);
                return;
            }

            newLocation.x -= 1;
            newLocation = ClampLayoutLocation(newLocation);
            characterGroup.Select(currentLayout[newLocation.x][newLocation.y]);
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (currentSelectedLocation.x == currentLayout.Count - 1)
            {
                characterGroup.Select(characterGroup.Items[^1]);
                return;
            }

            var newLocation = currentSelectedLocation;
            newLocation.x += 1;
            newLocation = ClampLayoutLocation(newLocation);
            characterGroup.Select(currentLayout[newLocation.x][newLocation.y]);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            characterGroup.Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            storyController.ExitCharacterVoiceCharacterSelection();
        }
    }
}