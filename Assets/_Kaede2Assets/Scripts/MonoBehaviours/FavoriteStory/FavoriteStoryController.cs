using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class FavoriteStoryController : StorySelectionSceneController
    {
        [SerializeField]
        private LabeledListSelectableGroup categorySelectableGroup;

        [SerializeField]
        private TextMeshProUGUI noFavoriteMessage;

        private Dictionary<MainStoryKey, MainStoryProvider> mainStoryProviders;
        private Dictionary<MasterScenarioInfo.Kind, OtherStoryProvider> otherStoryProviders;

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

        private void InitializeCategorySelection()
        {
            if (SaveData.FavoriteScenarioNames.Count == 0)
            {
                noFavoriteMessage.gameObject.SetActive(true);
                categorySelectableGroup.transform.parent.gameObject.SetActive(false);
                return;
            }

            noFavoriteMessage.gameObject.SetActive(false);
    
            var favoriteCategoryInfos = MasterScenarioInfo.Instance.Data
                .Where(si => SaveData.FavoriteScenarioNames.Contains(si.ScenarioName))
                .OrderBy(si => si.ChapterId)
                .ThenBy(si => si.EpisodeId)
                .ToList();

            categorySelectableGroup.Clear();
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
                            EnterEpisodeSelection(mainProvider);
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
                            EnterEpisodeSelection(otherProvider);
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
    }
}