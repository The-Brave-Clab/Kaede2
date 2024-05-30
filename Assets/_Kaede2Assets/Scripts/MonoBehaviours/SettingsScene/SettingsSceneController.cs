using System;
using System.Collections;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class SettingsSceneController : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return SceneTransition.Fade(0);
        }
    }
}