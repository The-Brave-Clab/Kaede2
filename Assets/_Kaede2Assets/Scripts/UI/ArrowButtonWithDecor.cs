using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class ArrowButtonWithDecor : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private Image foreground;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            foreground.color = theme.ArrowWithDecor;
        }
    }
}