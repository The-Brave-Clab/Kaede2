using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.UI
{
    public class TabViewHorizontal : TabView
    {
        protected override IEnumerator ActivateViewCoroutine(int index)
        {
            var backupCurrentViewIndex = currentViewIndex;
            currentViewIndex = index; // update current view index immediately to prevent early coroutine stop

            var nextView = views[index];

            nextView.gameObject.SetActive(true);

            if (index < backupCurrentViewIndex)
            {
                var contentPos = content.anchoredPosition;
                contentPos.x -= nextView.rect.width;
                content.anchoredPosition = contentPos;
            }

            var currentPosX = content.anchoredPosition.x;
            // wait a frame to let the layout group update the content position
            yield return null;
            var targetPos = -nextView.anchoredPosition.x;

            activateViewSequence = DOTween.Sequence();
            activateViewSequence.Append(DOVirtual.Float(currentPosX, targetPos, 0.2f, value =>
            {
                var contentPos = content.anchoredPosition;
                contentPos.x = value;
                content.anchoredPosition = contentPos;
            }));

            yield return activateViewSequence.WaitForCompletion();

            // activate only the selected view
            for (var i = 0; i < views.Length; i++)
            {
                views[i].gameObject.SetActive(i == index);
            }

            content.anchoredPosition = Vector2.zero;

            activateViewCoroutine = null;
            activateViewSequence = null;
        }
    }
}