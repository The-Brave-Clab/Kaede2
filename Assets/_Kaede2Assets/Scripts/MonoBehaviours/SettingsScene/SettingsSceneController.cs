using System;
using System.Collections;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class SettingsSceneController : MonoBehaviour
    {
        public bool FontsLoaded { get; set; }

        private void Awake()
        {
            FontsLoaded = false;
        }

        private IEnumerator Start()
        {
            while (!FontsLoaded)
            {
                yield return null;
            }

            yield return SceneTransition.Fade(0);
        }
    }
}