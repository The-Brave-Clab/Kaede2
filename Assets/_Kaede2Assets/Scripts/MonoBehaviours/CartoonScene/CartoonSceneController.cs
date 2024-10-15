using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Audio;
using Kaede2.Input;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CommonUtils = Kaede2.Utils.CommonUtils;

namespace Kaede2
{
    public class CartoonSceneController : MonoBehaviour, Kaede2InputAction.ICartoonActions
    {
        [SerializeField]
        private RandomizeScatterImages scatterImages;

        [SerializeField]
        private CartoonChapterSelection cartoonFramePrefab;

        [SerializeField]
        private Canvas chapterSelectionCanvas;

        [SerializeField]
        private ScrollRect chapterScroll;

        [SerializeField]
        private CartoonEpisodeSelection episodeSelection;

        [SerializeField]
        private Canvas cartoonViewCanvas;

        [SerializeField]
        private CartoonViewWindow cartoonViewWindow;

        private GridLayoutGroup chapterGrid;
        private int currentSelectedIndex;

        private class CartoonInfoGroupIdComparer : IEqualityComparer<MasterCartoonInfo.CartoonInfo>
        {
            public bool Equals(MasterCartoonInfo.CartoonInfo x, MasterCartoonInfo.CartoonInfo y)
            {
                return x?.GroupId == y?.GroupId;
            }

            public int GetHashCode(MasterCartoonInfo.CartoonInfo obj)
            {
                return obj.GroupId.GetHashCode();
            }
        }

        private void Awake()
        {
            chapterSelectionCanvas.gameObject.SetActive(true);
            episodeSelection.gameObject.SetActive(false);
            cartoonViewCanvas.gameObject.SetActive(false);

            chapterGrid = chapterScroll.content.GetComponent<GridLayoutGroup>();
            currentSelectedIndex = 0;
        }

        private IEnumerator Start()
        {
            var cartoonChapters = MasterCartoonInfo.Instance.Data
                .Distinct(new CartoonInfoGroupIdComparer())
                .OrderBy(ci => ci.No)
                .ToList();

            var group = new CoroutineGroup();

            group.Add(new WaitUntil(() => scatterImages.Loaded));

            for (int i = 0; i < cartoonChapters.Count; i++)
            {
                var chapterIndex = i;
                var chapterSelection = Instantiate(cartoonFramePrefab, chapterScroll.content);
                chapterSelection.onSelect.AddListener(() => { currentSelectedIndex = chapterIndex; });
                group.Add(chapterSelection.Initialize(this, i + 1));
            }

            yield return group.WaitForAll();

            yield return SceneTransition.Fade(0);

            InputManager.InputAction.Cartoon.Enable();
            InputManager.InputAction.Cartoon.AddCallbacks(this);
        }

        private void OnDestroy()
        {
            if (InputManager.InputAction == null) return;

            InputManager.InputAction.Cartoon.RemoveCallbacks(this);
            InputManager.InputAction.Cartoon.Disable();
            InputManager.InputAction.CartoonEpisode.RemoveCallbacks(episodeSelection);
            InputManager.InputAction.CartoonEpisode.Disable();
            InputManager.InputAction.CartoonView.RemoveCallbacks(cartoonViewWindow);
            InputManager.InputAction.CartoonView.Disable();
        }

        public void OnChapterSelected(CartoonChapterSelection chapterSelection)
        {
            StartCoroutine(OnChapterSelectedCoroutine(chapterSelection));
        }

        private IEnumerator OnChapterSelectedCoroutine(CartoonChapterSelection chapterSelection)
        {
            InputManager.InputAction.Cartoon.Disable();
            yield return SceneTransition.Fade(1);

            chapterSelectionCanvas.gameObject.SetActive(false);
            episodeSelection.gameObject.SetActive(true);
            cartoonViewCanvas.gameObject.SetActive(false);

            yield return episodeSelection.Initialize(this, chapterSelection);

            yield return SceneTransition.Fade(0);
            InputManager.InputAction.CartoonEpisode.Enable();
            InputManager.InputAction.CartoonEpisode.SetCallbacks(episodeSelection);
        }

        public void BackToChapterSelection()
        {
            StartCoroutine(BackToChapterSelectionCoroutine());
        }

        private IEnumerator BackToChapterSelectionCoroutine()
        {
            InputManager.InputAction.CartoonEpisode.RemoveCallbacks(episodeSelection);
            InputManager.InputAction.CartoonEpisode.Disable();
            yield return SceneTransition.Fade(1);

            chapterSelectionCanvas.gameObject.SetActive(true);
            episodeSelection.gameObject.SetActive(false);
            cartoonViewCanvas.gameObject.SetActive(false);

            yield return SceneTransition.Fade(0);
            InputManager.InputAction.Cartoon.Enable();
        }

        public void OnEpisodeSelected(MasterCartoonInfo.CartoonInfo cartoonInfo)
        {
            StartCoroutine(OnEpisodeSelectedCoroutine(cartoonInfo));
        }

        private IEnumerator OnEpisodeSelectedCoroutine(MasterCartoonInfo.CartoonInfo cartoonInfo)
        {
            InputManager.InputAction.CartoonEpisode.Disable();

            yield return cartoonViewWindow.Initialize(cartoonInfo, this);

            chapterSelectionCanvas.gameObject.SetActive(false);
            episodeSelection.gameObject.SetActive(false);
            cartoonViewCanvas.gameObject.SetActive(true);

            InputManager.InputAction.CartoonView.Enable();
            InputManager.InputAction.CartoonView.SetCallbacks(cartoonViewWindow);
        }

        public void BackToEpisodeSelection()
        {
            InputManager.InputAction.CartoonView.RemoveCallbacks(cartoonViewWindow);
            InputManager.InputAction.CartoonView.Disable();

            chapterSelectionCanvas.gameObject.SetActive(false);
            episodeSelection.gameObject.SetActive(true);
            cartoonViewCanvas.gameObject.SetActive(false);

            InputManager.InputAction.CartoonEpisode.Enable();
        }

        public void BackToMainMenu()
        {
            AudioManager.PlayRandomSystemVoice(MasterSystemVoiceData.VoiceCategory.BackToMainMenu);
            CommonUtils.LoadNextScene("MainMenuScene", LoadSceneMode.Single);
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var maxGridLocation = chapterGrid.GetMaxColumnRowCount();
            var currentGridLocation = chapterGrid.GetLocationFromChild(chapterGrid.transform.GetChild(currentSelectedIndex));
            currentGridLocation.y -= 1;
            if (currentGridLocation.y < 0) currentGridLocation.y = maxGridLocation.y - 1;

            var item = chapterGrid.GetChildFromLocation(currentGridLocation);
            chapterScroll.MoveItemIntoViewportSmooth(item as RectTransform, 0.1f, 0.1f);
            item.GetComponent<CartoonChapterSelection>().Select();
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var maxGridLocation = chapterGrid.GetMaxColumnRowCount();
            var currentGridLocation = chapterGrid.GetLocationFromChild(chapterGrid.transform.GetChild(currentSelectedIndex));
            currentGridLocation.y += 1;
            if (currentGridLocation.y >= maxGridLocation.y) currentGridLocation.y = 0;

            var item = chapterGrid.GetChildFromLocation(currentGridLocation);
            chapterScroll.MoveItemIntoViewportSmooth(item as RectTransform, 0.1f, 0.1f);
            item.GetComponent<CartoonChapterSelection>().Select();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var maxGridLocation = chapterGrid.GetMaxColumnRowCount();
            var currentGridLocation = chapterGrid.GetLocationFromChild(chapterGrid.transform.GetChild(currentSelectedIndex));
            currentGridLocation.x -= 1;
            if (currentGridLocation.x < 0) currentGridLocation.x = maxGridLocation.x - 1;

            var item = chapterGrid.GetChildFromLocation(currentGridLocation);
            item.GetComponent<CartoonChapterSelection>().Select();
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var maxGridLocation = chapterGrid.GetMaxColumnRowCount();
            var currentGridLocation = chapterGrid.GetLocationFromChild(chapterGrid.transform.GetChild(currentSelectedIndex));
            currentGridLocation.x += 1;
            if (currentGridLocation.x >= maxGridLocation.x) currentGridLocation.x = 0;

            var item = chapterGrid.GetChildFromLocation(currentGridLocation);
            item.GetComponent<CartoonChapterSelection>().Select();
        }

        public void OnConfirm(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            OnChapterSelected(CartoonChapterSelection.CurrentSelected);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            BackToMainMenu();
        }
    }
}
