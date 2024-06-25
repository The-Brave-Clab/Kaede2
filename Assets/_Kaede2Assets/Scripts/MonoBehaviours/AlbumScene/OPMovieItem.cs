using System;
using Kaede2.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

namespace Kaede2
{
    public class OPMovieItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField]
        private GameObject selectedOutline;

        [SerializeField]
        private string title;

        [SerializeField]
        private TMP_FontAsset titleFont;

        [SerializeField]
        private VideoClip videoClip;

        [SerializeField]
        private Canvas opMovieCanvas;

        [SerializeField]
        private OpeningMoviePlayer opMoviePlayer;

        private static OPMovieItem currentSelected = null;

        public string Title
        {
            get => title;
            set => title = value;
        }

        public TMP_FontAsset TitleFont
        {
            get => titleFont;
            set => titleFont = value;
        }

        private void OnEnable()
        {
            InputManager.onDeviceTypeChanged += OnInputDeviceChanged;

            UpdateSelectionVisibleStatus(false);
        }

        private void OnDisable()
        {
            InputManager.onDeviceTypeChanged -= OnInputDeviceChanged;

            Deselect();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            opMovieCanvas.gameObject.SetActive(true);
            opMoviePlayer.Play(videoClip);
        }

        private void Select()
        {
            if (currentSelected == this) return;

            if (currentSelected != null)
                currentSelected.Deselect(); 
            
            currentSelected = this;

            AlbumTitle.Text = title;
            AlbumTitle.Font = titleFont;
            UpdateSelectionVisibleStatus(true);
        }

        private void Deselect()
        {
            if (currentSelected == this)
                currentSelected = null;

            UpdateSelectionVisibleStatus(false);
        }

        public void UpdateSelectionVisibleStatus(bool selected)
        {
            selectedOutline.SetActive(InputManager.CurrentDeviceType != InputDeviceType.Touchscreen && selected);
        }

        private void OnInputDeviceChanged(InputDeviceType type)
        {
            UpdateSelectionVisibleStatus(currentSelected == this);
        }
    }
}