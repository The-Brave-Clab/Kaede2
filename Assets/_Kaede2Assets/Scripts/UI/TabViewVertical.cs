using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Kaede2.UI
{
    public class TabViewVertical : TabView
    {
        protected override IEnumerator ActivateViewCoroutine(int index)
        {
            var backupCurrentViewIndex = currentViewIndex;
            currentViewIndex = index; // update current view index right now to prevent early coroutine stop

            var nextView = views[index];

            nextView.gameObject.SetActive(true);

            if (index < backupCurrentViewIndex)
            {
                var contentPos = content.anchoredPosition;
                contentPos.y += nextView.rect.height;
                content.anchoredPosition = contentPos;
            }

            var currentPosY = content.anchoredPosition.y;
            // wait a frame to let the layout group update the content position
            yield return null;
            var targetPos = -nextView.anchoredPosition.y;

            activateViewSequence = DOTween.Sequence();
            activateViewSequence.Append(DOVirtual.Float(currentPosY, targetPos, 0.2f, value =>
            {
                var contentPos = content.anchoredPosition;
                contentPos.y = value;
                content.anchoredPosition = contentPos;
            }));

            yield return activateViewSequence.WaitForCompletion();

            // do a full scan and activate only the selected view
            // since this coroutine might be stopped before it finishes and leave some old views active
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