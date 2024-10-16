using System;
using System.Globalization;
using System.Linq;
using Kaede2.Audio;
using Kaede2.Input;
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

        [SerializeField]
        private TranslationStatus translationStatus;

        private LabeledListSelectableItem selectable;
        private StorySelectionSceneController eventStoryController;
        private MasterScenarioInfo.ScenarioInfo scenarioInfo;
        private MasterScenarioCast.ScenarioCast castInfo;

        public bool Read => SaveData.ReadScenarioNames.Contains(scenarioInfo.ScenarioName);
        public FavoriteIcon FavoriteIcon => favoriteIcon;

        private void Start()
        {
            InputManager.onDeviceTypeChanged += Refresh;
        }

        private void OnDestroy()
        {
            InputManager.onDeviceTypeChanged -= Refresh;
        }

        public void Refresh()
        {
            Refresh(InputManager.CurrentDeviceType);
        }

        private void Refresh(InputDeviceType deviceType)
        {
            if (scenarioInfo == null) return;
            bool read = SaveData.ReadScenarioNames.Contains(scenarioInfo.ScenarioName);
            unreadIcon.enabled = !read;
            if (read && selectable != null)
                favoriteIcon.gameObject.SetActive(deviceType == InputDeviceType.Touchscreen || selectable.selected);
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

            translationStatus.SetScenario(info);
        }

        private void OnFavoriteIconClicked()
        {
            if (IsFavorite())
            {
                SaveData.RemoveFavoriteScenario(scenarioInfo);
                AudioManager.CancelSound();
            }
            else
            {
                SaveData.AddFavoriteScenario(scenarioInfo);
                AudioManager.ConfirmSound();
            }
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
                favoriteIcon.gameObject.SetActive(InputManager.CurrentDeviceType == InputDeviceType.Touchscreen);
        }

        private void OnSelectableConfirmed()
        {
            CultureInfo locale = LocalizationManager.CurrentLocale;
            if (translationStatus.Status != ScriptTranslationManager.LoadStatus.Success)
                locale = LocalizationManager.AllLocales.First();
            CoroutineProxy.Start(
                eventStoryController.EnterScenario(scenarioInfo, locale)
            );
        }
    }
}