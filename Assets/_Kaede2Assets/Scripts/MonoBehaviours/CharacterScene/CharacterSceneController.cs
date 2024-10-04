using System;
using System.Collections;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kaede2
{
    public class CharacterSceneController : MonoBehaviour
    {
        [SerializeField]
        private ZukanViewWindow zukanViewWindow;
        public ZukanViewWindow ZukanViewWindow => zukanViewWindow;

        [Header("Character Selection")]
        [SerializeField]
        private CharacterSelection characterSelectionPrefab;

        [SerializeField]
        private RectTransform characterSelectionContainer;

        [SerializeField]
        private Image characterPreviewImage;
        public Image CharacterPreviewImage => characterPreviewImage;

        [SerializeField]
        private CharacterProfileController characterProfileController;
        public CharacterProfileController CharacterProfileController => characterProfileController;

        [Header("Fairy Selection")]
        [SerializeField]
        private FairySelection fairySelectionPrefab;

        [SerializeField]
        private RectTransform fairySelectionContainer;

        [SerializeField]
        private Image fairyPreviewImage;
        public Image FairyPreviewImage => fairyPreviewImage;

        [Header("Vertex Selection")]
        [SerializeField]
        private VertexSelection vertexSelectionPrefab;

        [SerializeField]
        private RectTransform vertexSelectionContainer;

        [SerializeField]
        private Image vertexPreviewImage;
        public Image VertexPreviewImage => vertexPreviewImage;

        [Header("Udon Selection")]
        [SerializeField]
        private UdonSelection udonSelectionPrefab;

        [SerializeField]
        private RectTransform udonSelectionContainer;

        [SerializeField]
        private Image udonPreviewImage;
        public Image UdonPreviewImage => udonPreviewImage;

        [Header("Stardust Selection")]
        [SerializeField]
        private StardustSelection stardustSelectionPrefab;

        [SerializeField]
        private RectTransform stardustSelectionContainer;

        [SerializeField]
        private Image stardustPreviewImage;
        public Image StardustPreviewImage => stardustPreviewImage;

        private void Awake()
        {
            zukanViewWindow.gameObject.SetActive(false);
            characterProfileController.gameObject.SetActive(false);
        }

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

            // vertex selection
            vertexPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var vertexProfile in MasterZukanVertexProfile.Instance.zukanProfile)
            {
                if (string.IsNullOrEmpty(vertexProfile.BigPicture)) continue;

                VertexSelection selection = Instantiate(vertexSelectionPrefab, vertexSelectionContainer);
                selection.gameObject.name = vertexProfile.Name;
                group.Add(selection.Initialize(this, vertexProfile, first));
                first = false;
            }

            // udon selection
            udonPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var udonProfile in MasterZukanUdonProfile.Instance.zukanProfile)
            {
                if (string.IsNullOrEmpty(udonProfile.Thumbnail)) continue;

                UdonSelection selection = Instantiate(udonSelectionPrefab, udonSelectionContainer);
                selection.gameObject.name = udonProfile.Name;
                group.Add(selection.Initialize(this, udonProfile, first));
                first = false;
            }

            // stardust selection
            stardustPreviewImage.gameObject.SetActive(false);
            first = true;
            foreach (var stardustProfile in MasterZukanStardustProfile.Instance.zukanProfile)
            {
                if (string.IsNullOrEmpty(stardustProfile.StandingPic)) continue;

                StardustSelection selection = Instantiate(stardustSelectionPrefab, stardustSelectionContainer);
                selection.gameObject.name = stardustProfile.Name;
                group.Add(selection.Initialize(this, stardustProfile, first));
                first = false;
            }

            yield return group.WaitForAll();

            yield return SceneTransition.Fade(0);
        }
    }
}