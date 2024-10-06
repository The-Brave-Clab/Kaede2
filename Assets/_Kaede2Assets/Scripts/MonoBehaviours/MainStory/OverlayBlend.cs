using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class OverlayBlend : AdjustHSV, IThemeChangeObserver
    {
        protected override string shaderName => "UI/UI Overlay Blend";

        [SerializeField]
        private bool isMainOverlay;

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            OnThemeChange(Theme.Current);

            base.Awake();
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            adjustment = isMainOverlay ? theme.MainOverlay : theme.SubOverlay;
        }
    }
}