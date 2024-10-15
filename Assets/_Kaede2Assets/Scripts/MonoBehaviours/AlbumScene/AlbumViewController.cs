using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using Kaede2.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CommonUtils = Kaede2.Utils.CommonUtils;

namespace Kaede2
{
    public class AlbumViewController : MonoBehaviour, Kaede2InputAction.IAlbumActions
    {
        [SerializeField]
        private GameObject albumItemPrefab;

        [SerializeField]
        private float unloadAssetInterval = 10;

        [SerializeField]
        private TabGroup tabGroup;

        [SerializeField]
        private CommonButton[] tabButtons;

        [SerializeField]
        private ScrollRect illustScrollRect;

        [SerializeField]
        private List<OPMovieItem> opMovieItems;

        [SerializeField]
        private LabeledListSelectableGroup bgmSelectableGroup;

        [SerializeField]
        private TMP_FontAsset bgmTitleFont;

        [SerializeField]
        private OpeningMoviePlayer moviePlayer;

        private MasterAlbumInfo masterData;

        private List<AlbumItem> albumItems;

        private GridLayoutGroup illustGrid;

        private float unloadAssetTimer;

        private Func<MasterAlbumInfo.AlbumInfo, bool> currentFilter;

        private bool focusInTabs;
        private int currentSelectedTabAndButtonIndex;

        private void Awake()
        {
            masterData = MasterAlbumInfo.Instance;

            focusInTabs = false;
            currentSelectedTabAndButtonIndex = 0;
    
            illustGrid = illustScrollRect.content.GetComponent<GridLayoutGroup>();

            albumItems = new()
            {
                Capacity = masterData.Data.Length
            };

            currentFilter = info => true;

            int order = 0;
            foreach (var album in MasterAlbumInfo.Sorted)
            {
                var albumItem = Instantiate(albumItemPrefab, illustScrollRect.content).GetComponent<AlbumItem>();
                albumItem.gameObject.name = $"{album.AlbumName} [{album.ViewName}]";
                albumItem.transform.SetSiblingIndex(order);
                albumItem.AlbumInfo = album;
                albumItem.Scroll = illustScrollRect;
                albumItem.onSelected.AddListener(() => { focusInTabs = false; });
                if (order == 0) albumItem.Select(true);
                albumItem.UpdateSelectionVisibleStatus(order == 0);
                albumItems.Add(albumItem);
                ++order;
            }

            foreach (var item in opMovieItems)
            {
                item.onSelected.AddListener(() => { focusInTabs = false; });
                item.onConfirmed.AddListener(() =>
                {
                    InputManager.InputAction.Album.Disable();
                });
            }

            moviePlayer.onOpeningMovieFinished.AddListener(() =>
            {
                InputManager.InputAction.Album.Enable();
            });

            for (int i = 0; i < tabButtons.Length; ++i)
            {
                int buttonIndex = i;
                int tabCount = tabGroup.Items.Count;

                tabButtons[i].onActivate.AddListener(() =>
                {
                    focusInTabs = true;
                    currentSelectedTabAndButtonIndex = buttonIndex + tabCount;
                });
            }

            for (var i = 0; i < tabGroup.Items.Count; i++)
            {
                var tabIndex = i;
                tabGroup.Items[i].onSelected.AddListener(() =>
                {
                    focusInTabs = true;
                    currentSelectedTabAndButtonIndex = tabIndex;
                });
            }

            unloadAssetTimer = unloadAssetInterval;

            foreach (var bgmData in MasterBgmData.Instance.Data.OrderBy(bd => bd.id))
            {
                var bgmItem = bgmSelectableGroup.Add("", bgmData.bgmTitle);
                bgmItem.onSelected.AddListener(() =>
                {
                    AlbumTitle.Text = bgmData.bgmTitle;
                    AlbumTitle.Font = bgmTitleFont;
                    focusInTabs = false;
                });
                bgmItem.onConfirmed.AddListener(() =>
                {
                    // TODO: play bgm
                    // AudioManager.Instance.PlayBgm(bgmData.cueName);
                    this.Log($"Play BGM: {bgmData.cueName}");
                });
            }

            bgmSelectableGroup.Initialize();
        }

        private IEnumerator Start()
        {
            yield return null;
            illustScrollRect.verticalNormalizedPosition = 1;

            yield return null;

            yield return SceneTransition.Fade(0);
        }

        private void Update()
        {
            unloadAssetTimer -= Time.deltaTime;
            if (unloadAssetTimer <= 0)
            {
                unloadAssetTimer = unloadAssetInterval;
                Resources.UnloadUnusedAssets();
            }
        }

        private void OnEnable()
        {
            InputManager.InputAction.Album.AddCallbacks(this);
            InputManager.InputAction.Album.Enable();
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.Album.Disable();
            InputManager.InputAction.Album.SetCallbacks(null);
        }

        public void SetFilter(Func<MasterAlbumInfo.AlbumInfo, bool> filter)
        {
            currentFilter = filter ?? (_ => true);

            AlbumItem firstItem = null;
            foreach (var albumItem in albumItems)
            {
                var filterResult = currentFilter(albumItem.AlbumInfo);
                albumItem.gameObject.SetActive(filterResult);

                if (filterResult && firstItem == null)
                    firstItem = albumItem;
            }

            var selected = AlbumItem.CurrentSelected;
            if (selected == null || !currentFilter(selected.AlbumInfo))
                selected = firstItem;

            if (selected == null)
                return;

            // we use a coroutine to ensure that the selected item is visible one frame after it's set to active
            IEnumerator ForceSelectedVisibleCoroutine(AlbumItem item)
            {
                illustScrollRect.StopMovement();
                yield return null;
                item.Select(true);
            }

            CoroutineProxy.Start(ForceSelectedVisibleCoroutine(selected));
        }

        private AlbumItem GetFirst()
        {
            return albumItems.FirstOrDefault(item => currentFilter(item.AlbumInfo));
        }

        public AlbumItem GetPrevious()
        {
            var current = AlbumItem.CurrentSelected;
            if (current == null) return GetFirst();

            var index = albumItems.IndexOf(current);
            if (index == -1) return GetFirst();

            if (index == 0) return null;

            for (int i = index - 1; i >= 0; --i)
                if (currentFilter(albumItems[i].AlbumInfo))
                    return albumItems[i];

            return null;
        }

        public AlbumItem GetNext()
        {
            var current = AlbumItem.CurrentSelected;
            if (current == null) return GetFirst();

            var index = albumItems.IndexOf(current);
            if (index == -1) return GetFirst();

            if (index == albumItems.Count - 1) return null;

            for (int i = index + 1; i < albumItems.Count; ++i)
                if (currentFilter(albumItems[i].AlbumInfo))
                    return albumItems[i];

            return null;
        }

        public void BackToMainMenu()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.BackToMainMenu);
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs)
            {
                int newSelectedIndex = currentSelectedTabAndButtonIndex - 1;
                newSelectedIndex = Mathf.Clamp(newSelectedIndex, 0, tabGroup.Items.Count + tabButtons.Length - 1);

                if (newSelectedIndex == currentSelectedTabAndButtonIndex) return;

                if (currentSelectedTabAndButtonIndex >= tabGroup.Items.Count)
                {
                    tabButtons[currentSelectedTabAndButtonIndex - tabGroup.Items.Count].OnPointerExit(null);
                }

                while (newSelectedIndex >= tabGroup.Items.Count && !tabButtons[newSelectedIndex - tabGroup.Items.Count].Interactable)
                    --newSelectedIndex;

                if (newSelectedIndex >= tabGroup.Items.Count)
                {
                    tabButtons[newSelectedIndex - tabGroup.Items.Count].OnPointerEnter(null);
                }
                else
                {
                    tabGroup.Select(newSelectedIndex);
                }
            }
            else
            {
                if (tabGroup.ActiveIndex == 0) // illustrations
                {
                    var selected = AlbumItem.CurrentSelected;
                    var maxLocation = illustGrid.GetMaxColumnRowCount();
                    var currentLocation = illustGrid.GetLocationFromChild(selected.transform);

                    var newLocation = currentLocation;
                    newLocation.y -= 1;

                    if (newLocation.y < 0) newLocation.y = maxLocation.y - 1;

                    var newSelected = illustGrid.GetChildFromLocation(newLocation);
                    newSelected.GetComponent<AlbumItem>().Select(true);
                }
                else if (tabGroup.ActiveIndex == 2) // bgm
                {
                    bgmSelectableGroup.ShouldMoveItemIntoViewPort();
                    bgmSelectableGroup.Previous();
                }
            }
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs)
            {
                int newSelectedIndex = currentSelectedTabAndButtonIndex + 1;
                newSelectedIndex = Mathf.Clamp(newSelectedIndex, 0, tabGroup.Items.Count + tabButtons.Length - 1);

                if (newSelectedIndex == currentSelectedTabAndButtonIndex) return;

                if (currentSelectedTabAndButtonIndex >= tabGroup.Items.Count)
                {
                    tabButtons[currentSelectedTabAndButtonIndex - tabGroup.Items.Count].OnPointerExit(null);
                }
                else if (currentSelectedTabAndButtonIndex == tabGroup.Items.Count - 1)
                {
                    tabGroup.DeselectAll();
                }

                if (newSelectedIndex >= tabGroup.Items.Count)
                {
                    while (!tabButtons[newSelectedIndex - tabGroup.Items.Count].Interactable)
                        ++newSelectedIndex;
                    tabButtons[newSelectedIndex - tabGroup.Items.Count].OnPointerEnter(null);
                }
                else
                {
                    tabGroup.Select(newSelectedIndex);
                }
            }
            else
            {
                if (tabGroup.ActiveIndex == 0) // illustrations
                {
                    var selected = AlbumItem.CurrentSelected;
                    var maxLocation = illustGrid.GetMaxColumnRowCount();
                    var currentLocation = illustGrid.GetLocationFromChild(selected.transform);

                    var newLocation = currentLocation;
                    newLocation.y += 1;

                    if (newLocation.y >= maxLocation.y) newLocation.y = 0;

                    var newSelected = illustGrid.GetChildFromLocation(newLocation);
                    newSelected.GetComponent<AlbumItem>().Select(true);
                }
                else if (tabGroup.ActiveIndex == 2) // bgm
                {
                    bgmSelectableGroup.ShouldMoveItemIntoViewPort();
                    bgmSelectableGroup.Next();
                }
            }
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs) return;

            void GoToTabs()
            {
                int activeIndex = 0;
                foreach (var selectableItem in tabGroup.Items)
                {
                    var tab = selectableItem as TabItem;
                    if (tab == null) continue;
                    if (tab.Active) break;
                    ++activeIndex;
                }

                tabGroup.Select(tabGroup.Items[activeIndex]);
            }

            if (tabGroup.ActiveIndex == 0) // illustrations
            {
                var selected = AlbumItem.CurrentSelected;
                var maxLocation = illustGrid.GetMaxColumnRowCount();
                var currentLocation = illustGrid.GetLocationFromChild(selected.transform);

                if (currentLocation.x == 0)
                {
                    selected.Deselect();
                    GoToTabs();
                }
                else
                {
                    var newLocation = currentLocation;
                    newLocation.x -= 1;

                    if (newLocation.x < 0) newLocation.x = maxLocation.x - 1;

                    var newSelected = illustGrid.GetChildFromLocation(newLocation);
                    newSelected.GetComponent<AlbumItem>().Select(true);
                }
            }
            else if (tabGroup.ActiveIndex == 1) // op movie
            {
                var currentSelectedIndex = opMovieItems.IndexOf(OPMovieItem.CurrentSelected);
                if (currentSelectedIndex == -1)
                {
                    opMovieItems[0].Select();
                    return;
                }

                if (currentSelectedIndex == 0)
                {
                    opMovieItems[0].Deselect();
                    GoToTabs();
                }
                else
                {
                    opMovieItems[currentSelectedIndex - 1].Select();
                }
            }
            else if (tabGroup.ActiveIndex == 2) // bgm
            {
                bgmSelectableGroup.DeselectAll();
                GoToTabs();
            }
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs)
            {
                if (currentSelectedTabAndButtonIndex >= tabGroup.Items.Count)
                {
                    tabButtons[currentSelectedTabAndButtonIndex - tabGroup.Items.Count].OnPointerExit(null);
                }
                else if (currentSelectedTabAndButtonIndex < tabGroup.Items.Count)
                {
                    tabGroup.DeselectAll();
                }

                if (tabGroup.ActiveIndex == 0) // illustrations
                {
                    AlbumItem.CurrentSelected.Select(false);
                }
                else if (tabGroup.ActiveIndex == 1) // op movie
                {
                    if (OPMovieItem.CurrentSelected == null)
                        opMovieItems[0].Select();
                    else
                        OPMovieItem.CurrentSelected.Select();
                }
                else if (tabGroup.ActiveIndex == 2) // bgm
                {
                    bgmSelectableGroup.Select(bgmSelectableGroup.LastSelected);
                }

                return;
            }

            if (tabGroup.ActiveIndex == 0) // illustrations
            {
                var selected = AlbumItem.CurrentSelected;
                var maxLocation = illustGrid.GetMaxColumnRowCount();
                var currentLocation = illustGrid.GetLocationFromChild(selected.transform);

                var newLocation = currentLocation;
                newLocation.x += 1;

                if (newLocation.x >= maxLocation.x) newLocation.x = 0;
                if (newLocation.y * maxLocation.x + newLocation.x >= albumItems.Count) newLocation.x = 0;

                var newSelected = illustGrid.GetChildFromLocation(newLocation);
                newSelected.GetComponent<AlbumItem>().Select(false); // go horizontally won't make the item out of view
            }
            else if (tabGroup.ActiveIndex == 1) // op movie
            {
                var currentSelectedIndex = opMovieItems.IndexOf(OPMovieItem.CurrentSelected);
                if (currentSelectedIndex == -1)
                {
                    opMovieItems[0].Select();
                    return;
                }

                var newSelectedIndex = currentSelectedIndex + 1;
                if (newSelectedIndex >= opMovieItems.Count) newSelectedIndex = 0;
                opMovieItems[newSelectedIndex].Select();
            }
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs)
            {
                if (currentSelectedTabAndButtonIndex >= tabGroup.Items.Count)
                {
                    var button = tabButtons[currentSelectedTabAndButtonIndex - tabGroup.Items.Count];
                    button.OnPointerClick(null);
                    button.OnPointerExit(null);
                }
                else
                {
                    tabGroup.Confirm();
                    tabGroup.DeselectAll();
                    focusInTabs = false;

                    if (currentSelectedTabAndButtonIndex == 0) // illustrations
                    {
                        AlbumItem.CurrentSelected.OnPointerEnter(null);
                    }
                    else if (currentSelectedTabAndButtonIndex == 1) // op movie
                    {
                        opMovieItems[0].Select();
                    }
                    else if (currentSelectedTabAndButtonIndex == 2) // bgm
                    {
                        bgmSelectableGroup.Select(bgmSelectableGroup.LastSelected);
                    }
                }
            }
            else
            {
                if (tabGroup.ActiveIndex == 0) // illustrations
                {
                    AlbumItem.CurrentSelected.OnPointerClick(null);
                }
                else if (tabGroup.ActiveIndex == 1) // op movie
                {
                    OPMovieItem.CurrentSelected.OnPointerClick(null);
                }
                else if (tabGroup.ActiveIndex == 2) // bgm
                {
                    bgmSelectableGroup.Confirm();
                }
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            BackToMainMenu();
        }

        public void OnFavorite(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs) return;

            if (tabGroup.ActiveIndex != 0) return;

            AlbumItem.CurrentSelected.FavoriteIcon.OnPointerClick(null);
        }

        public void OnSet(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusInTabs) return;
            if (tabGroup.ActiveIndex == 0)
            {
                AlbumItem.CurrentSelected.WallpaperIcon.OnPointerClick(null);
            }

            // TODO: BGM
        }
    }
}