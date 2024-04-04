using Kaede2.ScriptableObjects;

namespace Kaede2.UI
{
    public interface IThemeChangeObserver
    {
        void OnThemeChange(Theme.VolumeTheme theme);
    }
}