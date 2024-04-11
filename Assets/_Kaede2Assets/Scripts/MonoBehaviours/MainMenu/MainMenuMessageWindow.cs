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

        private Coroutine changeTextCoroutine;

        public void ChangeText(string newText)
        {
            if (changeTextCoroutine != null)
                StopCoroutine(changeTextCoroutine);

            changeTextCoroutine = StartCoroutine(ChangeTextCoroutine(newText));
        }

        private IEnumerator ChangeTextCoroutine(string newText)
        {
            int index = 0;
            while (index < newText.Length)
            {
                // we need to have the text laid out before starting the typewriter effect
                // so we embrace the not-shown text with color tag <color=#FFF0></color>

                string shownPart = newText[..index];
                string notShownPart = newText[index..];
                text.text = $"{shownPart}<color=#FFF0>{notShownPart}</color>";
                yield return new WaitForSeconds(0.01f);
                index++;
            }

            text.text = newText;
        }
    }
}
