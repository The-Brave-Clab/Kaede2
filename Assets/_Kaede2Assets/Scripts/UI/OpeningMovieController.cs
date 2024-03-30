using System;
using Kaede2.Input;
using Kaede2.Scenario;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Kaede2.UI
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

            Debug.Log($"Playing opening movie: {GameSettings.OpeningMovie}");
            videoPlayer.clip = openingMovies[GameSettings.OpeningMovie];
            videoPlayer.Play();
    
            OnOpeningMovieFinished += () =>
            {
                Debug.Log($"Opening movie finished: {GameSettings.OpeningMovie}");
#if UNITY_IOS
                UnityEngine.iOS.Device.hideHomeButton = false;
#endif

                PlayerScenarioModule.GlobalScenarioName = "ms006_s011_a";
                SceneManager.LoadScene("ScenarioScene", LoadSceneMode.Single);
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
