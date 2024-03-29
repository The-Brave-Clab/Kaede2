using System;
using UnityEngine;
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

            int index = GameSettings.OpeningMovie == 0 ? UnityEngine.Random.Range(0, 2) : GameSettings.OpeningMovie - 1;
            videoPlayer.clip = openingMovies[index];
            videoPlayer.Play();
    
            OnOpeningMovieFinished += () =>
            {
                Debug.Log($"Opening movie finished: {index}");
            };
        }
    }
}
