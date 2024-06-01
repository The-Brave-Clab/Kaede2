using System;
using System.Collections;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class SettingsSceneController : MonoBehaviour
    {
        public static event Action goBackAction;

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