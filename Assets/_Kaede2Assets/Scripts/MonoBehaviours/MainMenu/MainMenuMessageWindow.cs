using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kaede2.Scenario.Framework.UI;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class MainMenuMessageWindow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private Coroutine changeTextCoroutine;

        private RichText currentText;

        private void Awake()
        {
            currentText = "";
        }

        public void ChangeText(string newText)
        {
            if (string.Equals(currentText.MacroText, newText))
                return;

            currentText = newText;
            if (changeTextCoroutine != null)
                StopCoroutine(changeTextCoroutine);

            changeTextCoroutine = StartCoroutine(ChangeTextCoroutine());
        }

        private IEnumerator ChangeTextCoroutine()
        {
            int index = 0;
            while (index < currentText.Length)
            {
                // we need to have the text laid out before starting the typewriter effect
                // so we embrace the not-shown text with color tag <color=#FFF0></color>

                string shownPart = currentText.Substring(0, index);
                string hiddenPart = currentText.Substring(index, currentText.Length - index);
                text.text = $"{shownPart}<color=#FFF0>{hiddenPart}</color>";
                yield return new WaitForSeconds(0.01f);
                index++;
            }

            text.text = currentText.ToString();
        }
    }
}
