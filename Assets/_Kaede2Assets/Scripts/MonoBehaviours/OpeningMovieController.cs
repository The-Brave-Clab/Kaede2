using System;
using System.Collections;
using Kaede2.Input;
using Kaede2.UI;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Kaede2
{
    public class OpeningMovieController : MonoBehaviour
    {
        [SerializeField]
        private VideoClip[] openingMovies;

        [SerializeField]
        private VideoPlayer videoPlayer;

        public Action OnOpeningMovieFinished;

        private static GameSettings.OpeningMovieOptions? openingMovie = null;

        private void Awake()
        {
            if (openingMovies.Length != (int) GameSettings.OpeningMovieOptions.Count)
            {
                this.LogWarning("Opening movie count in code does not match the serialized item count!");
            }

            openingMovie ??= GameSettings.OpeningMovie == GameSettings.OpeningMovieOptions.Random
                ? (GameSettings.OpeningMovieOptions)UnityEngine.Random.Range(0, openingMovies.Length)
                : GameSettings.OpeningMovie;
    
            OnOpeningMovieFinished += () =>
            {
                this.Log($"Opening movie finished: {openingMovie:G}");
                CommonUtils.LoadNextScene("TitleScene", LoadSceneMode.Single);
            };

            if (openingMovie == GameSettings.OpeningMovieOptions.Disabled)
            {
                OnOpeningMovieFinished?.Invoke();
                return;
            }

            videoPlayer.loopPointReached += _ => { OnOpeningMovieFinished?.Invoke(); };

            this.Log($"Playing opening movie: {openingMovie:G}");
            videoPlayer.clip = openingMovies[(int)openingMovie];
            videoPlayer.Play();
        }

        private void OnEnable()
        {
            InputManager.InputAction.SplashScreen.Enable();
            InputManager.InputAction.SplashScreen.Skip.performed += OnSkipPerformed;
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = true;
#endif
        }

        private void OnDisable()
        {
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = false;
#endif
            InputManager.InputAction.SplashScreen.Skip.performed -= OnSkipPerformed;
            InputManager.InputAction?.SplashScreen.Disable();
        }

        private void OnSkipPerformed(InputAction.CallbackContext ctx)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();

            OnOpeningMovieFinished?.Invoke();
        }
    }
}
