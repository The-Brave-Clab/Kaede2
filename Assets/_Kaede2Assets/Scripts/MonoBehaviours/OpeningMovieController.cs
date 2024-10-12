using Kaede2.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Kaede2
{
    public class OpeningMovieController : MonoBehaviour
    {
        [SerializeField]
        private OpeningMoviePlayer openingMoviePlayer;

        [SerializeField]
        private VideoClip[] openingMovies;

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
    
            openingMoviePlayer.onOpeningMovieFinished.AddListener(() =>
            {
                this.Log($"Opening movie finished: {openingMovie:G}");
                CommonUtils.LoadNextScene("TitleScene", LoadSceneMode.Single);
            });

            if (openingMovie == GameSettings.OpeningMovieOptions.Disabled)
            {
                openingMoviePlayer.Play(null);
            }
            else
            {
                this.Log($"Playing opening movie: {openingMovie:G}");
                openingMoviePlayer.Play(openingMovies[(int)openingMovie]);
            }
        }
    }
}