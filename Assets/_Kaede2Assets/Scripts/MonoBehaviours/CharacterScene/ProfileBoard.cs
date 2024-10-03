using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class ProfileBoard : MonoBehaviour, IThemeChangeObserver
    {
        [SerializeField]
        private AdjustHSV hsv;

        private void Awake()
        {
            OnThemeChange(Theme.Current);
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            hsv.adjustment = theme.CharacterProfile;
        }
    }
}