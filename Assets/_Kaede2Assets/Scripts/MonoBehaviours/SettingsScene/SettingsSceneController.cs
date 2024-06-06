using System;
using System.Collections;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class SettingsSceneController : MonoBehaviour
    {
        public static event Action goBackAction;

        [SerializeField]
        private GameObject displayTab;

        private void Awake()
        {
            // hide the display tab on mobile platforms
#if !UNITY_EDITOR && !UNITY_STANDALONE
            displayTab.SetActive(false);
#endif
        }

        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);
        }

        private static void GoBackInternal()
        {
            goBackAction?.Invoke();
            goBackAction = null;
        }

        public void GoBack()
        {
            GoBackInternal();
        }
    }
}