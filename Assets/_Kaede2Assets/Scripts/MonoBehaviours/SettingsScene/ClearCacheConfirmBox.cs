using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    public class ClearCacheConfirmBox : SettingConfirmBox
    {
        private void Awake()
        {
            yesButton.onClick.AddListener(() =>
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                Caching.ClearCache();
                this.Log("Cache cleared");
#endif
#if UNITY_EDITOR
                UnityEditor.EditorApplication.ExitPlaymode();
#else
                Application.Quit(0);
#endif
            });

            noButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}