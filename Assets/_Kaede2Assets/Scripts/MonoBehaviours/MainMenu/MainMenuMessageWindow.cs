using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class MainMenuMessageWindow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private int textIndex;
        private RectTransform textRT;
        private RectTransform textContainer;
        private Coroutine changeTextCoroutine;
        private Sequence changeTextSequence;

        private void Awake()
        {
            textContainer = text.transform.parent as RectTransform;
            textRT = text.GetComponent<RectTransform>();
            textIndex = 0;
        }

        public void ChangeText(int index, string newText)
        {
            if (textIndex == index)
            {
                text.text = newText;
                return;
            }

            bool directionIsDown = index < textIndex;
            textIndex = index;

            if (changeTextCoroutine != null)
                StopCoroutine(changeTextCoroutine);

            changeTextCoroutine = StartCoroutine(ChangeTextCoroutine(newText, directionIsDown));
        }

        private IEnumerator ChangeTextCoroutine(string newText, bool directionIsDown)
        {
            List<GameObject> allTextObjects = new List<GameObject>();
            foreach (Transform child in textContainer)
            {
                allTextObjects.Add(child.gameObject);
            }

            var newGameObject = Instantiate(text.gameObject, textContainer);
            newGameObject.name = "Text";
            text = newGameObject.GetComponent<TextMeshProUGUI>();
            text.text = newText;
            textRT = text.GetComponent<RectTransform>();
            if (directionIsDown)
                textRT.SetAsFirstSibling();
            else
                textRT.SetAsLastSibling();

            float from = textContainer.anchoredPosition.y;
            float to = directionIsDown ? 0 : textRT.sizeDelta.y * allTextObjects.Count;
            if (directionIsDown)
            {
                from += textRT.sizeDelta.y;
                textRT.anchoredPosition = new Vector2(textRT.anchoredPosition.x, from);
            }

            if (changeTextSequence != null)
                changeTextSequence.Kill();

            changeTextSequence = DOTween.Sequence();
            changeTextSequence.Append(DOVirtual.Float(from, to, 0.2f, value =>
            {
                var position = textContainer.anchoredPosition;
                position.y = value;
                textContainer.anchoredPosition = position;
            }));
            changeTextSequence.SetEase(Ease.OutQuad);

            yield return changeTextSequence.WaitForCompletion();
            changeTextSequence = null;

            foreach (var obj in allTextObjects)
            {
                Destroy(obj);
            }
            textContainer.anchoredPosition = new Vector2(textContainer.anchoredPosition.x, 0);
        }
    }
}
