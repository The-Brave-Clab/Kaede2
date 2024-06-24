using System.Collections;
using DG.Tweening;
using Kaede2.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2
{
    public class FavoriteIcon : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private RemapRGB colorComponent;

        [SerializeField]
        private AlbumItem item;

        [SerializeField]
        private Color color;

        private Coroutine coroutine;
        private Sequence sequence;

        private void OnEnable()
        {
            var (c, o) = GetColor();
            colorComponent.targetColorRed = c;
            colorComponent.targetColorGreen = o;
            colorComponent.targetColorBlue = Color.black;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            item.IsFavorite = !item.IsFavorite;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                sequence.Kill();
                coroutine = null;
                sequence = null;
            }

            coroutine = StartCoroutine(ChangeColorCoroutine());
        }

        private IEnumerator ChangeColorCoroutine()
        {
            Color currentRed = colorComponent.targetColorRed;
            Color currentGreen = colorComponent.targetColorGreen;
            Color currentBlue = colorComponent.targetColorBlue;

            var (targetRed, targetGreen) = GetColor();
            var targetBlue = Color.black;

            sequence = DOTween.Sequence();
            sequence.Append(DOVirtual.Float(0, 1, 0.2f, value =>
            {
                colorComponent.targetColorRed = Color.Lerp(currentRed, targetRed, value);
                colorComponent.targetColorGreen = Color.Lerp(currentGreen, targetGreen, value);
                colorComponent.targetColorBlue = Color.Lerp(currentBlue, targetBlue, value);
            }));

            yield return sequence.WaitForCompletion();

            coroutine = null;
            sequence = null;
        }

        private (Color center, Color outline) GetColor()
        {
            bool isFavorite = item.IsFavorite;
            var c = isFavorite ? color : new Color(0, 0, 0, 0);
            var o = isFavorite ? Color.white : Color.black;
            return (c, o);
        }
    }
}
