using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Kaede2.Localization;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class CharacterWindow : MonoBehaviour
    {
        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private RectTransform contentContainer;

        [SerializeField]
        private RectTransform characterNamePrefab;

        [SerializeField]
        private RectTransform paddingPrefab;

        [SerializeField]
        private float characterNameHeight = 45;

        [SerializeField]
        private float paddingHeight = 30;

        [SerializeField]
        private float scrollSpeed = 50;

        [SerializeField]
        private float waitTime = 0.5f;

        private Coroutine coroutine;
        private List<CharacterId> characterIds;
        private List<TextMeshProUGUI> characterNameTexts;

        private void Awake()
        {
            Reset();

            LocalizationManager.OnLocaleChanged += LocalizeCharacterNames;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLocaleChanged -= LocalizeCharacterNames;
        }

        public void SetNames(IEnumerable<CharacterId> ids)
        {
            Reset();

            float currentHeight = 0;
            float maxHeight = contentContainer.rect.height;

            foreach (CharacterId characterId in ids)
            {
                characterIds.Add(characterId);

                RectTransform characterName = Instantiate(characterNamePrefab, contentContainer);
                characterName.gameObject.SetActive(true);
                characterName.sizeDelta = new Vector2(characterName.sizeDelta.x, characterNameHeight);

                TextMeshProUGUI text = characterName.GetComponentInChildren<TextMeshProUGUI>();
                text.text = characterNames.Get(characterId);
                characterNameTexts.Add(text);

                currentHeight += characterNameHeight;
            }

            if (currentHeight > maxHeight)
            {
                RectTransform padding = Instantiate(paddingPrefab, contentContainer);
                padding.gameObject.SetActive(true);
                padding.sizeDelta = new Vector2(padding.sizeDelta.x, paddingHeight);

                coroutine = StartCoroutine(ScrollContent());
            }
        }

        private void Reset()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            characterIds = new();
            characterNameTexts = new();

            // clear all children
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            // reset container height
            contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, 0);
        }

        private void LocalizeCharacterNames(CultureInfo locale)
        {
            for (var i = 0; i < characterNameTexts.Count; i++)
            {
                var text = characterNameTexts[i];
                var id = characterIds[i];
                text.text = characterNames.Get(id, locale);
            }
        }

        private IEnumerator ScrollContent()
        {
            yield return new WaitForSeconds(waitTime);

            while (true)
            {
                RectTransform firstChild = contentContainer.GetChild(0) as RectTransform;
                float childHeight = firstChild!.sizeDelta.y;

                float y = contentContainer.sizeDelta.y + Time.deltaTime * scrollSpeed;
                if (y < childHeight)
                {
                    contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, y);
                }
                else
                {
                    // move first child to the end
                    firstChild.SetSiblingIndex(contentContainer.childCount - 1);
                    contentContainer.sizeDelta = new Vector2(contentContainer.sizeDelta.x, y - childHeight);
                }

                yield return null;
            }
        }
    }
}
