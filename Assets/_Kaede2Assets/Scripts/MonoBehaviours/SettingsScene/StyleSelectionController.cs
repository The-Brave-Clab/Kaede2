using UnityEngine;

namespace Kaede2
{
    public class StyleSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        private void Awake()
        {
            selectionControl.SelectImmediate(GameSettings.ConsoleStyle ? 0 : 1, false);
        }

        // used by SelectionItem UnityEvent
        public void SelectStyle(bool consoleStyle)
        {
            GameSettings.ConsoleStyle = consoleStyle;
        }
    }
}