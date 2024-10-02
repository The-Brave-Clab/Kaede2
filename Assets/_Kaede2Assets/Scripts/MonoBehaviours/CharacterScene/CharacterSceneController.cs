using System;
using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;

namespace Kaede2
{
    public class CharacterSceneController : MonoBehaviour
    {
        [Header("Character Selection")]
        [SerializeField]
        private CharacterSelection characterSelectionPrefab;

        [SerializeField]
        private RectTransform characterSelectionContainer;

        private IEnumerator Start()
        {
            MasterCharaProfile profile = MasterCharaProfile.Instance;

            CoroutineGroup group = new();

            // character selection
            bool first = true;
            foreach (var characterProfile in profile.charaProfile)
            {
                if (string.IsNullOrEmpty(characterProfile.Thumbnail)) continue;

                CharacterSelection selection = Instantiate(characterSelectionPrefab, characterSelectionContainer);
                selection.gameObject.name = characterProfile.Name;
                group.Add(selection.Initialize(characterProfile, first));
                first = false;
            }

            yield return group.WaitForAll();

            yield return SceneTransition.Fade(0);
        }
    }
}