using System;
using System.Collections;
using System.Linq;
using Kaede2.Input;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public class SettingsSceneController : MonoBehaviour, Kaede2InputAction.ISettingActions
    {
        public static event Action goBackAction;

        [SerializeField]
        private GameObject displayTab;

        [SerializeField]
        private CommonButton displayApplyButton;

        [SerializeField]
        private TabGroup tabGroup;

        [SerializeField]
        private SettingsItem[] tab1Items;

        [SerializeField]
        private SettingsItem[] tab2Items;

        [SerializeField]
        private SettingsItem[] tab3Items;

        [SerializeField]
        private SettingsItem[] tab4Items;

        private SettingsItem[][] tabsItems;

        private bool focusOnTabs;
        private bool controlSettings;
        private int activeTabIndex;
        private SettingsItem currentSelected;
        private bool displayApplyButtonSelected;

        private void Awake()
        {
            // hide the display tab on mobile platforms
#if !UNITY_EDITOR && !UNITY_STANDALONE
            displayTab.SetActive(false);
#endif

            tabsItems = new SettingsItem[4][];
            tabsItems[0] = tab1Items;
            tabsItems[1] = tab2Items;
            tabsItems[2] = tab3Items;
            tabsItems[3] = tab4Items;

            foreach (var tabItems in tabsItems)
            {
                foreach (var tabItem in tabItems)
                {
                    tabItem.onPointerEnter.AddListener(() =>
                    {
                        focusOnTabs = false;
                        controlSettings = false;
                        currentSelected = tabItem;
                        displayApplyButtonSelected = false;
                    });

                    if (tabItem.Control != null)
                    {
                        tabItem.Control.onPointerEnter.AddListener(() =>
                        {
                            focusOnTabs = false;
                            controlSettings = true;
                            currentSelected = tabItem;
                            displayApplyButtonSelected = false;
                        });
                    }
                }
            }

            for (var i = 0; i < tabGroup.Items.Count; i++)
            {
                var tabIndex = i;
                var tab = tabGroup.Items[i];
                tab.onSelected.AddListener(() =>
                {
                    focusOnTabs = true;
                    controlSettings = false;
                    displayApplyButtonSelected = false;
                });
                tab.onConfirmed.AddListener(() =>
                {
                    activeTabIndex = tabIndex;
                });
            }

            displayApplyButton.onActivate.AddListener(() =>
            {
                focusOnTabs = false;
                controlSettings = false;
                currentSelected = null;
                displayApplyButtonSelected = true;
            });

            focusOnTabs = false;
            controlSettings = false;
            activeTabIndex = 0;
            currentSelected = null;
            displayApplyButtonSelected = false;
        }

        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);

            InputManager.InputAction.Setting.Enable();
            InputManager.InputAction.Setting.SetCallbacks(this);
        }

        private void OnDestroy()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.Setting.RemoveCallbacks(this);
            InputManager.InputAction.Setting.Disable();
        }

        private static void GoBackInternal()
        {
            goBackAction?.Invoke();
            goBackAction = null;
        }

        public void GoBack()
        {
            GoBackInternal();
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnTabs)
            {
                tabGroup.Previous();
                return;
            }

            if (controlSettings) return;

            if ((currentSelected == null && !displayApplyButtonSelected) || (currentSelected != null && !tabsItems[activeTabIndex].Contains(currentSelected)))
            {
                tabsItems[activeTabIndex][0].OnPointerEnter(null);
                return;
            }

            if (activeTabIndex == 1 && displayApplyButtonSelected)
            {
                displayApplyButton.OnPointerExit(null);
                tabsItems[1][^1].OnPointerEnter(null);
                return;
            }

            var currentIndex = Array.IndexOf(tabsItems[activeTabIndex], currentSelected);
            var newIndex = Mathf.Clamp(currentIndex - 1, 0, tabsItems[activeTabIndex].Length - 1);
            if (currentIndex == newIndex) return;

            currentSelected.OnPointerExit(null);
            tabsItems[activeTabIndex][newIndex].OnPointerEnter(null);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnTabs)
            {
                tabGroup.Next();
                return;
            }

            if (controlSettings) return;

            if (currentSelected == null && displayApplyButtonSelected) return;

            if (!tabsItems[activeTabIndex].Contains(currentSelected))
            {
                tabsItems[activeTabIndex][0].OnPointerEnter(null);
                return;
            }

            if (activeTabIndex == 1 && currentSelected == tabsItems[1][^1] && displayApplyButton.Interactable)
            {
                tabsItems[1][^1].OnPointerExit(null);
                displayApplyButton.OnPointerEnter(null);
                return;
            }
    
            var currentIndex = Array.IndexOf(tabsItems[activeTabIndex], currentSelected);
            var newIndex = Mathf.Clamp(currentIndex + 1, 0, tabsItems[activeTabIndex].Length - 1);
            if (currentIndex == newIndex) return;

            currentSelected.OnPointerExit(null);
            tabsItems[activeTabIndex][newIndex].OnPointerEnter(null);
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnTabs) return;
            if (controlSettings && currentSelected != null && currentSelected.Control != null)
            {
                currentSelected.Control.Left();
            }
            else
            {
                if (currentSelected != null)
                    currentSelected.OnPointerExit(null);
                tabGroup.Select(tabGroup.ActiveIndex);
            }
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnTabs)
            {
                tabGroup.DeselectAll();
                if (tabsItems[activeTabIndex].Contains(currentSelected))
                {
                    currentSelected.OnPointerEnter(null);
                }
                else
                {
                    tabsItems[activeTabIndex][0].OnPointerEnter(null);
                }
            }
            else if (controlSettings && currentSelected != null && currentSelected.Control != null)
            {
                currentSelected.Control.Right();
            }
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (focusOnTabs)
            {
                tabGroup.Confirm();
                return;
            }

            if (activeTabIndex == 1 && displayApplyButtonSelected && displayApplyButton.Interactable)
            {
                displayApplyButton.OnPointerClick(null);
                return;
            }

            if (currentSelected == null && !displayApplyButtonSelected) return;

            if (activeTabIndex == 3 && currentSelected.Control == null)
            {
                var clearCache = currentSelected.GetComponent<ClearCacheSettingsItemController>();
                if (clearCache != null)
                {
                    clearCache.OnPointerClick(null);
                }
            }

            if (currentSelected.Control == null) return;

            if (controlSettings)
            {
                currentSelected.Control.OnPointerExit(null);
                currentSelected.OnPointerEnter(null);
                return;
            }

            currentSelected.Control.OnPointerEnter(null);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (controlSettings)
            {
                currentSelected.Control.OnPointerExit(null);
                currentSelected.OnPointerEnter(null);
                return;
            }

            GoBack();
        }
    }
}