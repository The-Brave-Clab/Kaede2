using Kaede2.Scenario.Framework.Utils;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kaede2
{
    public abstract class CharacterSceneBaseSelection : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public abstract void Select();

        public abstract void Confirm();

        public abstract void Deactivate();

        private Coroutine coroutine;

        public void Select(ScrollRect scrollRect)
        {
            Select();
            if (coroutine != null)
            {
                CoroutineProxy.Stop(coroutine);
                coroutine = null;
            }
            coroutine = scrollRect.MoveItemIntoViewportSmooth(transform as RectTransform, 0.1f, 0.1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Confirm();
        }
    }
}