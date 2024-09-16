using System.Collections;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Kaede2.Localization;
using Kaede2.Scenario.Framework;
using Kaede2.ScriptableObjects;
using Kaede2.UI.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class CharacterFilterButton : MonoBehaviour
    {
        [SerializeField]
        private CommonButton button;

        [SerializeField]
        private Image onImage;

        [SerializeField]
        private Image offImage;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private CharacterNames characterNames;

        [SerializeField]
        private CharacterId characterId;

        private Coroutine coroutine;
        private Sequence sequence;

        private bool filterActive;

        private void Awake()
        {
            filterActive = false;

            button.onClick.AddListener(() =>
            {
                filterActive = characterId == CharacterId.Unknown || !filterActive;
                FilterSettings.SetCharacterFilter(characterId, filterActive);
                button.Highlighted = filterActive;

                if (!FilterSettings.CharacterFilter.Any(pair => pair.Value))
                {
                    FilterSettings.CharacterFilterButtons[CharacterId.Unknown].button.Highlighted = true;
                }

                SetStatus();
            });

            LocalizationManager.OnLocaleChanged += SetCharacterName;
            SetCharacterName(LocalizationManager.CurrentLocale);

            coroutine = null;
            sequence = null;

            onImage.gameObject.SetActive(button.Activated);
            offImage.gameObject.SetActive(!button.Activated);
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLocaleChanged -= SetCharacterName;
        }

        private void Start()
        {
            FilterSettings.RegisterCharacterFilterButton(characterId, this);
        }

        public void Deactivate()
        {
            filterActive = false;
            button.Highlighted = false;
            SetStatus();
        }

        private void SetStatus()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                sequence.Kill();
                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(SetStatusCoroutine());
        }

        private IEnumerator SetStatusCoroutine()
        {
            Color currentOffColor = offImage.color;
            Color targetOffColor = new Color(1, 1, 1, filterActive ? 0 : 1);

            onImage.gameObject.SetActive(true);
            offImage.gameObject.SetActive(true);

            sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(0, 1, 0.2f, value => { offImage.color = Color.Lerp(currentOffColor, targetOffColor, value); }));
            yield return sequence.WaitForCompletion();

            onImage.gameObject.SetActive(filterActive);
            offImage.gameObject.SetActive(!filterActive);

            coroutine = null;
            sequence = null;
        }

        private void SetCharacterName(CultureInfo cultureInfo)
        {
            if (characterId == CharacterId.Unknown) return;
            text.text = characterNames.Get(characterId, cultureInfo);
        }
    }

}