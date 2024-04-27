using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaede2.Scenario
{
    public class ButtonForPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private bool lastPressed;
        private bool pressed;

        private bool pointerPressed;
        private bool pointerClicked;

        public bool Pressed => pressed && !lastPressed;

        private void Awake()
        {
            lastPressed = false;
            pointerPressed = false;
        }

        private void Update()
        {
            lastPressed = pressed;
            pressed = pointerPressed || pointerClicked;

            pointerClicked = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerPressed = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            pointerClicked = true;
        }
    }
}