using System;
using Kaede2.Input;
using Kaede2.Scenario;
using Kaede2.Utils;
using UnityEngine;
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

        private void Awake()
        {
            if (GameSettings.OpeningMovie < 0)
            {
                // opening movie is disabled
                OnOpeningMovieFinished?.Invoke();
                return;
            }

            videoPlayer.loopPointReached += _ => { OnOpeningMovieFinished?.Invoke(); };

            this.Log($"Playing opening movie: {GameSettings.OpeningMovie}");
            videoPlayer.clip = openingMovies[GameSettings.OpeningMovie];
            videoPlayer.Play();
    
            OnOpeningMovieFinished += () =>
            {
                this.Log($"Opening movie finished: {GameSettings.OpeningMovie}");
#if UNITY_IOS
                UnityEngine.iOS.Device.hideHomeButton = false;
#endif

                SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
            };
        }

        private void OnEnable()
        {
            InputManager.InputAction.SplashScreen.Enable();
            InputManager.InputAction.SplashScreen.Skip.performed += _ =>
            {
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                }

                OnOpeningMovieFinished?.Invoke();
            };
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = true;
#endif
        }

        private void OnDisable()
        {
            InputManager.InputAction.SplashScreen.Disable();
        }
    }
}
