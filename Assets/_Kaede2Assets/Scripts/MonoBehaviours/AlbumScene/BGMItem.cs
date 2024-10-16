using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class BGMItem : MonoBehaviour
    {
        [SerializeField]
        private LabeledListSelectableItem selectableItem;

        [SerializeField]
        private Image playingIcon;

        [SerializeField]
        private FavoriteIcon setIcon;

        public FavoriteIcon SetIcon => setIcon;

        private MasterBgmData.BgmData data;

        private static BGMItem currentBGM = null;
        private static BGMItem currentPlaying = null;

        private bool IsSet
        {
            get => SaveData.BGMName == data.cueName;
            set
            {
                if (!value) return;
                if (IsSet) return;

                SaveData.BGMName = data.cueName;
                if (currentBGM != null)
                {
                    currentBGM.setIcon.UpdateColor();
                    currentBGM.UpdateSelectionVisibleStatus(currentBGM.selectableItem.selected);
                }
                currentBGM = this;
                AudioManager.ConfirmSound();
            }
        }

        public void SetData(MasterBgmData.BgmData bgmData)
        {
            data = bgmData;

            selectableItem.onConfirmed.AddListener(SetPlayingIcon);
            selectableItem.onSelected.AddListener(() => UpdateSelectionVisibleStatus(true));
            selectableItem.onDeselected.AddListener(() => UpdateSelectionVisibleStatus(false));

            if (SaveData.BGMName == data.cueName) currentBGM = this;
            if (AudioManager.CurrentPlayingBGM == data.cueName) currentPlaying = this;

            setIcon.OnClicked = () => { IsSet = true; };
            setIcon.IsFavorite = () => IsSet;

            if (AudioManager.CurrentPlayingBGM == data.cueName)
                currentPlaying = this;
            playingIcon.gameObject.SetActive(AudioManager.CurrentPlayingBGM == data.cueName);
        }

        private void SetPlayingIcon()
        {
            if (currentPlaying != null && currentPlaying != this)
                currentPlaying.playingIcon.gameObject.SetActive(false);

            currentPlaying = this;
            playingIcon.gameObject.SetActive(true);
            AudioManager.ConfirmSound();
        }

        public void UpdateSelectionVisibleStatus(bool selected)
        {
            setIcon.gameObject.SetActive(InputManager.CurrentDeviceType == InputDeviceType.Touchscreen || selected || IsSet);
        }
    }
}