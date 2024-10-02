using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class CharacterSceneController : MonoBehaviour
    {
        [Header("Character Selection")]
        [SerializeField]
        private CharacterSelection characterSelectionPrefab;

        [SerializeField]
        private RectTransform characterSelectionContainer;

        [SerializeField]
        private Image characterPreviewImage;
        public Image CharacterPreviewImage => characterPreviewImage;

        [Header("Fairy Selection")]
        [SerializeField]
        private FairySelection fairySelectionPrefab;

        [SerializeField]
        private RectTransform fairySelectionContainer;

        [SerializeField]
        private Image fairyPreviewImage;
        public Image FairyPreviewImage => fairyPreviewImage;

        private IEnumerator Start()
        {
            CoroutineGroup group = new();

            // character selection
            characterPreviewImage.gameObject.SetActive(true);
            bool first = true;
            foreach (var characterProfile in MasterCharaProfile.Instance.charaProfile)
            {
                if (string.IsNullOrEmpty(characterProfile.Thumbnail)) continue;

                CharacterSelection selection = Instantiate(characterSelectionPrefab, characterSelectionContainer);
                selection.gameObject.name = characterProfile.Name;
                group.Add(selection.Initialize(this, characterProfile, first));
                first = false;
            }

            // fairy selection
            fairyPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var fairyProfile in MasterZukanFairyProfile.Instance.zukanProfile)
            {
                if (string.IsNullOrEmpty(fairyProfile.BigPicture)) continue;

                FairySelection selection = Instantiate(fairySelectionPrefab, fairySelectionContainer);
                selection.gameObject.name = fairyProfile.Name;
                group.Add(selection.Initialize(this, fairyProfile, first));
                first = false;
            }

            yield return group.WaitForAll();

            yield return SceneTransition.Fade(0);
        }
    }
}