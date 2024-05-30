using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kaede2.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using LocalizeStringEvent = UnityEngine.Localization.Components.LocalizeStringEvent;

namespace Kaede2
{
    public class LanguageSelectionController : MonoBehaviour
    {
        [SerializeField]
        private SelectionControl selectionControl;

        [SerializeField]
        private LocalizedTmpFont font;

        [SerializeField]
        private SettingsSceneController sceneController;

        private Dictionary<Locale, AsyncOperationHandle<TMP_FontAsset>> localeToFont;

        private void Awake()
        {
            localeToFont = new();
        }

        private IEnumerator Start()
        {
            // AsyncOperationHandle<TMP_FontAsset> handle;
            // handle.Acquire();
            // Acquire is an internal method, call with reflection
            Type handleType = typeof(AsyncOperationHandle<TMP_FontAsset>);
            var acquireMethod = handleType.GetMethod("Acquire", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                localeToFont[locale] = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TMP_FontAsset>(
                        font.TableReference, font.TableEntryReference, locale);
                yield return localeToFont[locale];
                acquireMethod!.Invoke(localeToFont[locale], null);
            }

            sceneController.FontsLoaded = true;

            int selectedIndex = -1;
            for (var i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                if (locale == GameSettings.Locale) selectedIndex = i;
                var selectionItem = selectionControl.Add("", () => GameSettings.Locale = locale);
                Destroy(selectionItem.gameObject.GetComponent<LocalizeStringEvent>());
                Destroy(selectionItem.gameObject.GetComponent<LocalizeFontEvent>());
                selectionItem.Font = localeToFont[locale].Result;
                selectionItem.Text = locale.Identifier.CultureInfo.NativeName;
            }

            selectionControl.SelectImmediate(selectedIndex, false);
        }

        private void OnDestroy()
        {
            foreach (var (_, handle) in localeToFont)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }
    }
}