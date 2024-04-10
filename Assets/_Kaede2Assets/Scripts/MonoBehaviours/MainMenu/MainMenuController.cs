using System.Collections;
using DG.Tweening;
using Kaede2.ScriptableObjects;
using Kaede2.UI;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class MainMenuController : SelectableGroup, IThemeChangeObserver
    {
        [SerializeField]
        private Image backgroundGradient;

        [SerializeField]
        private Image leftDecor;

        [SerializeField]
        private float menuYStart;

        [SerializeField]
        private float menuYStep;

        [SerializeField]
        private MainMenuMessageWindow messageWindow;

        private RectTransform rt;

        private Coroutine cursorMoveCoroutine;
        private Sequence cursorMoveSequence;

        protected override void Awake()
        {
            base.Awake();
            rt = GetComponent<RectTransform>();

            OnThemeChange(Theme.Current);

            for (int i = 0; i < Items.Count; ++i)
            {
                var index = i;
                Items[i].onSelected.AddListener(() => OnSelected(index));
                Items[i].onSelected.AddListener(() => messageWindow.ChangeText(index, (Items[index] as MainMenuSelectableItem)!.MessageWindowText));
            }
        }

        public void OnThemeChange(Theme.VolumeTheme theme)
        {
            backgroundGradient.color = theme.MainMenuGradientColor;
            leftDecor.color = theme.MainMenuLeftDecorColor;
        }

        private void OnSelected(int index)
        {
            if (cursorMoveCoroutine != null)
                StopCoroutine(cursorMoveCoroutine);

            cursorMoveCoroutine = StartCoroutine(MoveCursor(index));
        }

        private IEnumerator MoveCursor(int index)
        {
            var anchoredPos = rt.anchoredPosition;
            var targetY = menuYStart + index * menuYStep;

            cursorMoveSequence?.Kill();

            cursorMoveSequence = DOTween.Sequence();
            cursorMoveSequence.SetEase(Ease.OutQuad);
            cursorMoveSequence.Append(DOVirtual.Float(anchoredPos.y, targetY, 0.2f, value =>
            {
                anchoredPos.y = value;
                rt.anchoredPosition = anchoredPos;
            }));

            yield return cursorMoveSequence.WaitForCompletion();
            cursorMoveSequence = null;
        }
    }
}