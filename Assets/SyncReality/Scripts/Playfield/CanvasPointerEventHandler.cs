using Syncreality.Playfield;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Syncreality.Playfield
{
    public class CanvasPointerEventHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {

        public PlayfieldInteraction playfieldInteraction;

    public void OnDrag(PointerEventData eventData)
        {
            playfieldInteraction.dragEvent.Invoke(eventData.pointerCurrentRaycast.worldPosition);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            playfieldInteraction.tapEvent.Invoke(eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            playfieldInteraction.releaseEvent.Invoke(eventData.pointerCurrentRaycast.worldPosition);
        }
    }
}
