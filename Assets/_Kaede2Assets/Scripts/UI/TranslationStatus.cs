using System;
using System.Collections;
using DG.Tweening;
using Kaede2.Localization;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class TranslationStatus : MonoBehaviour
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private Image statusIcon;

        [SerializeField]
        private Sprite successIcon;

        [SerializeField]
        private Color successColor;

        [SerializeField]
        private Sprite warningIcon;

        [SerializeField]
        private Color warningColor;

        [SerializeField]
        private Sprite failureIcon;

        [SerializeField]
        private Color failureColor;

        [SerializeField]
        private Sprite loadingIcon;

        [SerializeField]
        private Color loadingColor;

        [SerializeField]
        private float loadingIconRotationSpeed = 180f;

        public ScriptTranslationManager.LoadStatus Status { get; private set; }

        private Coroutine transitionCoroutine;
        private Sequence transitionSequence;

        private Coroutine loadingCoroutine;

        private MasterScenarioInfo.ScenarioInfo info;

        private void Awake()
        {
            if (LocalizationManager.CurrentLocale.Name == "ja")
            {
                Destroy(gameObject);
                return;
            }

            // SetScenario might be called before Awake
            if (info == null)
            {
                statusIcon.sprite = loadingIcon;
                statusIcon.color = loadingColor;
                Status = ScriptTranslationManager.LoadStatus.Loading;
            }
        }

        private void OnDestroy()
        {
            if (transitionCoroutine != null)
            {
                CoroutineProxy.Stop(transitionCoroutine);
                transitionSequence.Kill();
                transitionCoroutine = null;
                transitionSequence = null;
            }

            if (loadingCoroutine != null)
            {
                CoroutineProxy.Stop(loadingCoroutine);
                loadingCoroutine = null;
            }
        }

        public void SetScenario(MasterScenarioInfo.ScenarioInfo info)
        {
            if (this.info != null)
                return;

            this.info = info;

            if (info == null)
            {
                TransitionTo(failureIcon, failureColor);
                Status = ScriptTranslationManager.LoadStatus.Failure;
                return;
            }

            Status = ScriptTranslationManager.GetTranslationStatus(info.ScenarioName, LocalizationManager.CurrentLocale);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            switch (Status)
            {
                case ScriptTranslationManager.LoadStatus.Success:
                    statusIcon.transform.localRotation = Quaternion.identity;
                    TransitionTo(successIcon, successColor);
                    break;
                case ScriptTranslationManager.LoadStatus.Warning:
                    statusIcon.transform.localRotation = Quaternion.identity;
                    TransitionTo(warningIcon, warningColor);
                    break;
                case ScriptTranslationManager.LoadStatus.Failure:
                    statusIcon.transform.localRotation = Quaternion.identity;
                    TransitionTo(failureIcon, failureColor);
                    break;
                case ScriptTranslationManager.LoadStatus.Loading:
                    loadingCoroutine = CoroutineProxy.Start(LoadingCoroutine());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TransitionTo(Sprite sprite, Color color)
        {
            if (transitionCoroutine != null)
            {
                CoroutineProxy.Stop(transitionCoroutine);
                transitionSequence.Kill();
                transitionCoroutine = null;
                transitionSequence = null;
            }

            transitionCoroutine = CoroutineProxy.Start(TransitionToCoroutine(sprite, color));
        }

        private IEnumerator TransitionToCoroutine(Sprite sprite, Color color)
        {
            statusIcon.sprite = sprite;

            Color startColor = background.color;

            transitionSequence = DOTween.Sequence();
            transitionSequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
            {
                background.color = Color.Lerp(startColor, color, value);
            }));

            yield return transitionSequence.WaitForCompletion();

            transitionCoroutine = null;
            transitionSequence = null;
        }

        private IEnumerator LoadingCoroutine()
        {
            TransitionTo(loadingIcon, loadingColor);

            while (true)
            {
                Status = ScriptTranslationManager.GetTranslationStatus(info.ScenarioName, LocalizationManager.CurrentLocale);
                if (Status != ScriptTranslationManager.LoadStatus.Loading) break;

                statusIcon.transform.Rotate(Vector3.forward, -loadingIconRotationSpeed * Time.deltaTime);

                yield return null;
            }

            // status won't be loading anymore
            UpdateStatus();
        }
    }
}