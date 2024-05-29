using UnityEngine;

namespace Kaede2
{
    public class OpeningMovieSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        private void Awake()
        {
            selectionControl.SelectImmediate((int)GameSettings.OpeningMovie + 2, false);
        }

        // used by SelectionItem UnityEvent
        public void SelectOpeningMovie(int openingMovieIndex)
        {
            GameSettings.OpeningMovie = (GameSettings.OpeningMovieOptions)openingMovieIndex;
        }
    }
}