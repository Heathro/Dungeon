using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.HUD
{
    public class ButtonOnHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        // STATE

        Action onHold = null;
        Action sound = null;
        bool isHold = false;

        // LIFECYCLE

        void Update()
        {
            if (!isHold) return;

            if (onHold != null)
            {
                onHold();
            }
        }

        // PUBLIC

        public void SetOnHold(Action onHold, Action sound)
        {
            this.onHold = onHold;
            this.sound = sound;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            sound();
            isHold = true;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            isHold = false;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            isHold = false;
        }
    }
}