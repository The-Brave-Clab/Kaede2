using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class StorySelectionItem : MonoBehaviour
    {
        [SerializeField]
        private Image unreadIcon;

        [SerializeField]
        private FavoriteIcon favoriteIcon;

        private LabeledListSelectableItem selectable;
        private StorySelectionSceneController eventStoryController;
        private MasterScenarioInfo.ScenarioInfo scenarioInfo;
        private MasterScenarioCast.ScenarioCast castInfo;

        public void Refresh()
        {
            if (scenarioInfo == null) return;
            bool read = SaveData.ReadScenarioNames.Contains(scenarioInfo.ScenarioName);
            unreadIcon.enabled = !read;
            if (read && selectable != null)
                favoriteIcon.gameObject.SetActive(selectable.selected);
        }

        public void Initialize(MasterScenarioInfo.ScenarioInfo info, StorySelectionSceneController controller)
        {
            eventStoryController = controller;
            scenarioInfo = info;
            selectable = GetComponent<LabeledListSelectableItem>();
            castInfo = MasterScenarioCast.Instance.Data
                .First(c => c.ScenarioName == info.ScenarioName);

            unreadIcon.enabled = !SaveData.ReadScenarioNames.Contains(scenarioInfo.ScenarioName);
            favoriteIcon.gameObject.SetActive(!unreadIcon.enabled);
            favoriteIcon.OnClicked = OnFavoriteIconClicked;
            favoriteIcon.IsFavorite = IsFavorite;
            selectable.onSelected.AddListener(OnSelectableSelected);
            selectable.onDeselected.AddListener(OnSelectableDeselected);
            selectable.onConfirmed.AddListener(OnSelectableConfirmed);
        }

        private void OnFavoriteIconClicked()
        {
            if (IsFavorite())
                SaveData.RemoveFavoriteScenario(scenarioInfo);
            else
                SaveData.AddFavoriteScenario(scenarioInfo);
        }

        private bool IsFavorite()
        {
            return SaveData.IsScenarioFavorite(scenarioInfo);
        }

        private void OnSelectableSelected()
        {
            if (!unreadIcon.enabled)
                favoriteIcon.gameObject.SetActive(true);
            eventStoryController.CharacterWindow.SetNames(castInfo.CastCharaIds);
        }

        private void OnSelectableDeselected()
        {
            if (!unreadIcon.enabled)
                favoriteIcon.gameObject.SetActive(false);
        }

        private void OnSelectableConfirmed()
        {
            CoroutineProxy.Start(
                eventStoryController.EnterScenario(scenarioInfo, LocalizationManager.AllLocales.First())
            );
        }
    }
}