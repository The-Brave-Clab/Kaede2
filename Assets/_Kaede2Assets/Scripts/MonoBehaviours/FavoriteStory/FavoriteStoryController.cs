using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    public class FavoriteStoryController : StorySelectionSceneController, Kaede2InputAction.IFavoriteStoryActions
    {
        [SerializeField]
        private LabeledListSelectableGroup categorySelectableGroup;

        [SerializeField]
        private GameObject noFavoriteContainer;

        [SerializeField]
        private SelectableGroup noFavoriteGroup;

        [SerializeField]
        private SelectableItem noFavoriteBackButton;

        private Dictionary<MainStoryKey, MainStoryProvider> mainStoryProviders;
        private Dictionary<MasterScenarioInfo.Kind, OtherStoryProvider> otherStoryProviders;

        private MasterScenarioInfo.IProvider currentEpisodeProvider;

        protected override void Awake()
        {
            base.Awake();

            mainStoryProviders = new();
            otherStoryProviders = new();
        }

        private IEnumerator Start()
        {
            InitialSetup();

            InitializeCategorySelection();

            yield return null;
            yield return null;

            yield return SceneTransition.Fade(0);
        }

        private void OnEnable()
        {
            InputManager.InputAction.FavoriteStory.Enable();
            InputManager.InputAction.FavoriteStory.AddCallbacks(this);
        }

        private void OnDisable()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.FavoriteStory.RemoveCallbacks(this);
            InputManager.InputAction.FavoriteStory.Disable();
        }

        private void InitializeCategorySelection()
        {
            if (SaveData.FavoriteScenarioNames.Count == 0)
            {
                noFavoriteContainer.gameObject.SetActive(true);
                categorySelectableGroup.transform.parent.gameObject.SetActive(false);
                noFavoriteGroup.Select(noFavoriteBackButton);
                return;
            }

            noFavoriteContainer.gameObject.SetActive(false);
            categorySelectableGroup.transform.parent.gameObject.SetActive(true);
    
            var favoriteCategoryInfos = MasterScenarioInfo.Instance.Data
                .Where(si => SaveData.FavoriteScenarioNames.Contains(si.ScenarioName))
                .OrderBy(si => si.ChapterId)
                .ThenBy(si => si.EpisodeId)
                .ToList();

            categorySelectableGroup.Clear();
            mainStoryProviders.Clear();
            otherStoryProviders.Clear();
            foreach (var info in favoriteCategoryInfos)
            {
                if (info.KindId is MasterScenarioInfo.Kind.Main or MasterScenarioInfo.Kind.Sub)
                {
                    var key = new MainStoryKey { kindId = info.KindId, chapterId = info.ChapterId };
                    if (!mainStoryProviders.ContainsKey(key))
                    {
                        var mainProvider = new MainStoryProvider(key);
                        mainStoryProviders[key] = mainProvider;
                        var mainItem = categorySelectableGroup.Add("", $"{info.ChapterName} ({info.KindName})");
                        mainItem.onConfirmed.AddListener(() =>
                        {
                            currentEpisodeProvider = mainProvider;
                            EnterEpisodeSelection(mainProvider);
                            AudioManager.ConfirmSound();
                        });
                    }
                }
                else
                {
                    if (!otherStoryProviders.ContainsKey(info.KindId))
                    {
                        var otherProvider = new OtherStoryProvider(info.KindId);
                        otherStoryProviders[info.KindId] = otherProvider;
                        var otherItem = categorySelectableGroup.Add("", info.KindName);
                        otherItem.onConfirmed.AddListener(() =>
                        {
                            currentEpisodeProvider = otherProvider;
                            EnterEpisodeSelection(otherProvider);
                            AudioManager.ConfirmSound();
                        });
                    }
                }
            }

            categorySelectableGroup.Initialize();
        }

        protected override void InitialSetup()
        {
            base.InitialSetup();
            categorySelectableGroup.transform.parent.gameObject.SetActive(true);
        }

        protected override void OnEnterEpisodeSelection(MasterScenarioInfo.IProvider provider)
        {
            categorySelectableGroup.transform.parent.gameObject.SetActive(false);
        }

        protected override void OnExitEpisodeSelection()
        {
            InitializeCategorySelection();
        }

        protected override void OnExitStorySelection()
        {
            RefreshEpisodeSelection(currentEpisodeProvider);
        }

        protected override bool AdditionalStoryFilter(MasterScenarioInfo.ScenarioInfo scenario)
        {
            return SaveData.FavoriteScenarioNames.Contains(scenario.ScenarioName);
        }

        private struct MainStoryKey : IEquatable<MainStoryKey>
        {
            public MasterScenarioInfo.Kind kindId;
            public int chapterId;

            public bool Equals(MainStoryKey other)
            {
                return kindId == other.kindId && chapterId == other.chapterId;
            }

            public override bool Equals(object obj)
            {
                return obj is MainStoryKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine((int)kindId, chapterId);
            }
        }

        private class MainStoryProvider : MasterScenarioInfo.IProvider
        {
            private readonly MasterScenarioInfo.Kind kindId;
            private readonly int chapterId;

            public MainStoryProvider(MainStoryKey key)
            {
                kindId = key.kindId;
                chapterId = key.chapterId;
            }

            public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
            {
                return MasterScenarioInfo.Instance.Data
                    .Where(si => SaveData.FavoriteScenarioNames.Contains(si.ScenarioName))
                    .Where(si => si.KindId == kindId && si.ChapterId == chapterId);
            }
        }

        private class OtherStoryProvider : MasterScenarioInfo.IProvider
        {
            private readonly MasterScenarioInfo.Kind kindId;

            public OtherStoryProvider(MasterScenarioInfo.Kind kindId)
            {
                this.kindId = kindId;
            }

            public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
            {
                return MasterScenarioInfo.Instance.Data
                    .Where(si => SaveData.FavoriteScenarioNames.Contains(si.ScenarioName))
                    .Where(si => si.KindId == kindId);
            }
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            categorySelectableGroup.ShouldMoveItemIntoViewPort();
            if (categorySelectableGroup.Previous())
                AudioManager.ButtonSound();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            categorySelectableGroup.ShouldMoveItemIntoViewPort();
            if (categorySelectableGroup.Next())
                AudioManager.ButtonSound();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (SaveData.FavoriteScenarioNames.Count == 0) return;

            if (categorySelectableGroup.FocusedOnLabeledSelectables) return;

            categorySelectableGroup.FocusOnLabeledSelectables();
            AudioManager.ButtonSound();
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (SaveData.FavoriteScenarioNames.Count == 0) return;

            if (!categorySelectableGroup.FocusedOnLabeledSelectables) return;

            categorySelectableGroup.FocusOnAdditionalSelectableGroup();
            AudioManager.ButtonSound();
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            categorySelectableGroup.Confirm();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            BackToMainScene();
        }
    }
}