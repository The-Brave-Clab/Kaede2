using Kaede2.UI;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class ClearCacheConfirmBox : MonoBehaviour
    {
        [SerializeField]
        private BoxWindow boxWindow;

        [SerializeField]
        private CommonButton yesButton;

        [SerializeField]
        private CommonButton noButton;

        private void Awake()
        {
            yesButton.onClick.AddListener(() =>
            {
                Caching.ClearCache();
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