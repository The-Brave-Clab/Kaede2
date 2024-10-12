using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CommonUtils = Kaede2.Utils.CommonUtils;

namespace Kaede2
{
    public class CharacterSceneController : MonoBehaviour, Kaede2InputAction.ICharacterActions
    {
        [FormerlySerializedAs("tabgroup")] [SerializeField]
        private TabGroup tabGroup;

        [SerializeField]
        private ZukanViewWindow zukanViewWindow;
        public ZukanViewWindow ZukanViewWindow => zukanViewWindow;

        [Header("Character Selection")]
        [SerializeField]
        private CharacterSelection characterSelectionPrefab;
        private List<CharacterSceneBaseSelection> characterSelections;

        [SerializeField]
        private ScrollRect characterSelectionScrollRect;

        [SerializeField]
        private Image characterPreviewImage;
        public Image CharacterPreviewImage => characterPreviewImage;

        [SerializeField]
        private CharacterProfileController characterProfileController;
        public CharacterProfileController CharacterProfileController => characterProfileController;

        [SerializeField]
        private GameObject characterProfileObject;

        [Header("Fairy Selection")]
        [SerializeField]
        private FairySelection fairySelectionPrefab;
        private List<CharacterSceneBaseSelection> fairySelections;

        [SerializeField]
        private ScrollRect fairySelectionScrollRect;

        [SerializeField]
        private Image fairyPreviewImage;
        public Image FairyPreviewImage => fairyPreviewImage;

        [Header("Vertex Selection")]
        [SerializeField]
        private VertexSelection vertexSelectionPrefab;
        private List<CharacterSceneBaseSelection> vertexSelections;

        [SerializeField]
        private ScrollRect vertexSelectionScrollRect;

        [SerializeField]
        private Image vertexPreviewImage;
        public Image VertexPreviewImage => vertexPreviewImage;

        [Header("Udon Selection")]
        [SerializeField]
        private UdonSelection udonSelectionPrefab;
        private List<CharacterSceneBaseSelection> udonSelections;

        [SerializeField]
        private ScrollRect udonSelectionScrollRect;

        [SerializeField]
        private Image udonPreviewImage;
        public Image UdonPreviewImage => udonPreviewImage;

        [Header("Stardust Selection")]
        [SerializeField]
        private StardustSelection stardustSelectionPrefab;
        private List<CharacterSceneBaseSelection> stardustSelections;

        [SerializeField]
        private ScrollRect stardustSelectionScrollRect;

        [SerializeField]
        private Image stardustPreviewImage;
        public Image StardustPreviewImage => stardustPreviewImage;

        private List<CharacterSceneBaseSelection> currentSelections;
        private GridLayoutGroup currentGrid;
        private ScrollRect currentScrollRect;

        private bool focusOnGrid;

        private void Awake()
        {
            zukanViewWindow.gameObject.SetActive(false);
            characterProfileObject.SetActive(false);

            characterSelections = new();
            fairySelections = new();
            vertexSelections = new();
            udonSelections = new();
            stardustSelections = new();

            focusOnGrid = true;

            void DeactivateAllSelected()
            {
                CharacterSelection.Selected.Deactive();
                FairySelection.Selected.Deactive();
                VertexSelection.Selected.Deactive();
                UdonSelection.Selected.Deactive();
                StardustSelection.Selected.Deactive();
            }

            tabGroup.Items[0].onConfirmed.AddListener(() =>
            {
                currentSelections = characterSelections;
                currentGrid = characterSelectionScrollRect.content.GetComponent<GridLayoutGroup>();
                currentScrollRect = characterSelectionScrollRect;
            });

            tabGroup.Items[0].onSelected.AddListener(() =>
            {
                focusOnGrid = false;
                DeactivateAllSelected();
            });

            tabGroup.Items[1].onConfirmed.AddListener(() =>
            {
                currentSelections = fairySelections;
                currentGrid = fairySelectionScrollRect.content.GetComponent<GridLayoutGroup>();
                currentScrollRect = fairySelectionScrollRect;
            });

            tabGroup.Items[1].onSelected.AddListener(() =>
            {
                focusOnGrid = false;
                DeactivateAllSelected();
            });

            tabGroup.Items[2].onConfirmed.AddListener(() =>
            {
                currentSelections = vertexSelections;
                currentGrid = vertexSelectionScrollRect.content.GetComponent<GridLayoutGroup>();
                currentScrollRect = vertexSelectionScrollRect;
            });

            tabGroup.Items[2].onSelected.AddListener(() =>
            {
                focusOnGrid = false;
                DeactivateAllSelected();
            });

            tabGroup.Items[3].onConfirmed.AddListener(() =>
            {
                currentSelections = udonSelections;
                currentGrid = udonSelectionScrollRect.content.GetComponent<GridLayoutGroup>();
                currentScrollRect = udonSelectionScrollRect;
            });

            tabGroup.Items[3].onSelected.AddListener(() =>
            {
                focusOnGrid = false;
                DeactivateAllSelected();
            });

            tabGroup.Items[4].onConfirmed.AddListener(() =>
            {
                currentSelections = stardustSelections;
                currentGrid = stardustSelectionScrollRect.content.GetComponent<GridLayoutGroup>();
                currentScrollRect = stardustSelectionScrollRect;
            });

            tabGroup.Items[4].onSelected.AddListener(() =>
            {
                focusOnGrid = false;
                DeactivateAllSelected();
            });
        }

        private IEnumerator Start()
        {
            CoroutineGroup group = new();

            // character selection
            characterPreviewImage.gameObject.SetActive(true);
            bool first = true;
            foreach (var characterProfile in MasterCharaProfile.Instance.Data)
            {
                if (string.IsNullOrEmpty(characterProfile.Thumbnail)) continue;

                CharacterSelection selection = Instantiate(characterSelectionPrefab, characterSelectionScrollRect.content);
                selection.gameObject.name = characterProfile.Name;
                group.Add(selection.Initialize(this, characterProfile, first));
                characterSelections.Add(selection);
                first = false;
            }

            // fairy selection
            fairyPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var fairyProfile in MasterZukanFairyProfile.Instance.Data)
            {
                if (string.IsNullOrEmpty(fairyProfile.BigPicture)) continue;

                FairySelection selection = Instantiate(fairySelectionPrefab, fairySelectionScrollRect.content);
                selection.gameObject.name = fairyProfile.Name;
                group.Add(selection.Initialize(this, fairyProfile, first));
                fairySelections.Add(selection);
                first = false;
            }

            // vertex selection
            vertexPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var vertexProfile in MasterZukanVertexProfile.Instance.Data)
            {
                if (string.IsNullOrEmpty(vertexProfile.BigPicture)) continue;

                VertexSelection selection = Instantiate(vertexSelectionPrefab, vertexSelectionScrollRect.content);
                selection.gameObject.name = vertexProfile.Name;
                group.Add(selection.Initialize(this, vertexProfile, first));
                vertexSelections.Add(selection);
                first = false;
            }

            // udon selection
            udonPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var udonProfile in MasterZukanUdonProfile.Instance.Data)
            {
                if (string.IsNullOrEmpty(udonProfile.Thumbnail)) continue;

                UdonSelection selection = Instantiate(udonSelectionPrefab, udonSelectionScrollRect.content);
                selection.gameObject.name = udonProfile.Name;
                group.Add(selection.Initialize(this, udonProfile, first));
                udonSelections.Add(selection);
                first = false;
            }

            // stardust selection
            stardustPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var stardustProfile in MasterZukanStardustProfile.Instance.Data)
            {
                if (string.IsNullOrEmpty(stardustProfile.StandingPic)) continue;

                StardustSelection selection = Instantiate(stardustSelectionPrefab, stardustSelectionScrollRect.content);
                selection.gameObject.name = stardustProfile.Name;
                group.Add(selection.Initialize(this, stardustProfile, first));
                stardustSelections.Add(selection);
                first = false;
            }

            yield return group.WaitForAll();

            currentSelections = characterSelections;
            currentGrid = characterSelectionScrollRect.content.GetComponent<GridLayoutGroup>();
            currentScrollRect = characterSelectionScrollRect;

            yield return SceneTransition.Fade(0);
        }

        private void OnEnable()
        {
            InputManager.InputAction.Character.Enable();
            InputManager.InputAction.Character.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.Character.RemoveCallbacks(this);
            InputManager.InputAction.Character.Disable();
        }

        public void ItemSelected()
        {
            focusOnGrid = true;
        }

        public void BackToMainScene()
        {
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        private CharacterSceneBaseSelection GetSelected()
        {
            var activeTab = tabGroup.Items.Cast<TabItem>().FirstOrDefault(i => i.Active);
            if (activeTab == null) return null;
            var tabIndex = -1;

            for (var i = 0; i < tabGroup.Items.Count; i++)
            {
                if (tabGroup.Items[i] == activeTab)
                {
                    tabIndex = i;
                    break;
                }
            }

            return tabIndex switch
            {
                0 => CharacterSelection.Selected,
                1 => FairySelection.Selected,
                2 => VertexSelection.Selected,
                3 => UdonSelection.Selected,
                4 => StardustSelection.Selected,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (!focusOnGrid) return;

            CharacterSceneBaseSelection currentSelected = GetSelected();
            Transform currentSelectedObject = currentSelected.transform;

            var currentLocation = currentGrid.GetLocationFromChild(currentSelectedObject);

            var newLocation = currentLocation;
            newLocation.y -= 1;

            if (newLocation.y < 0)
            {
                currentSelected.Deactive();
                // select tab instead
                var activeTab = tabGroup.Items.Cast<TabItem>().FirstOrDefault(i => i.Active);
                if (activeTab != null) tabGroup.Select(activeTab);
            }
            else
            {
                var newObj = currentGrid.GetChildFromLocation(newLocation);
                if (newObj == null) return;
                CharacterSceneBaseSelection newSelected = newObj.GetComponent<CharacterSceneBaseSelection>();
                if (newSelected == null) return;
                newSelected.Select(currentScrollRect);
            }
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            CharacterSceneBaseSelection currentSelected = GetSelected();

            if (focusOnGrid)
            {
                Transform currentSelectedObject = currentSelected.transform;
                var maxLocation = currentGrid.GetMaxColumnRowCount();
                var currentLocation = currentGrid.GetLocationFromChild(currentSelectedObject);

                var newLocation = currentLocation;
                newLocation.y += 1;
                if (newLocation.y >= maxLocation.y || newLocation.y * maxLocation.x + newLocation.x >= currentGrid.transform.childCount)
                {
                    newLocation.y = 0;
                }

                var newObj = currentGrid.GetChildFromLocation(newLocation);
                if (newObj == null) return;
                CharacterSceneBaseSelection newSelected = newObj.GetComponent<CharacterSceneBaseSelection>();
                if (newSelected == null) return;
                newSelected.Select(currentScrollRect);
            }
            else
            {
                currentSelected.Select();
                tabGroup.DeselectAll();
            }
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnGrid)
            {
                var currentSelected = GetSelected();
                Transform currentSelectedObject = currentSelected.transform;
                var maxLocation = currentGrid.GetMaxColumnRowCount();
                var currentLocation = currentGrid.GetLocationFromChild(currentSelectedObject);
    
                var newLocation = currentLocation;
                newLocation.x -= 1;
                if (newLocation.x < 0)
                {
                    newLocation.x = maxLocation.x - 1;
                }

                var newObj = currentGrid.GetChildFromLocation(newLocation);
                if (newObj == null) return;
                CharacterSceneBaseSelection newSelected = newObj.GetComponent<CharacterSceneBaseSelection>();
                if (newSelected == null) return;
                newSelected.Select(currentScrollRect);
            }
            else
            {
                tabGroup.Previous();
            }
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnGrid)
            {
                var currentSelected = GetSelected();
                Transform currentSelectedObject = currentSelected.transform;
                var maxLocation = currentGrid.GetMaxColumnRowCount();
                var currentLocation = currentGrid.GetLocationFromChild(currentSelectedObject);
    
                var newLocation = currentLocation;
                newLocation.x += 1;
                if (newLocation.x >= maxLocation.x || newLocation.y * maxLocation.x + newLocation.x >= currentGrid.transform.childCount)
                {
                    newLocation.x = 0;
                }

                var newObj = currentGrid.GetChildFromLocation(newLocation);
                if (newObj == null) return;
                CharacterSceneBaseSelection newSelected = newObj.GetComponent<CharacterSceneBaseSelection>();
                if (newSelected == null) return;
                newSelected.Select(currentScrollRect);
            }
            else
            {
                tabGroup.Next();
            }
        }

        public void OnLeftShoulder(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var newIndex = tabGroup.SelectedIndex - 1;
            if (newIndex < 0) newIndex = tabGroup.Items.Count - 1;
            tabGroup.SelectTab(tabGroup.Items[newIndex]);
        }

        public void OnRightShoulder(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var newIndex = tabGroup.SelectedIndex + 1;
            if (newIndex >= tabGroup.Items.Count) newIndex = 0;
            tabGroup.SelectTab(tabGroup.Items[newIndex]);
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnGrid)
            {
                GetSelected().Confirm();
            }
            else
            {
                tabGroup.Confirm();
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            BackToMainScene();
        }
    }
}