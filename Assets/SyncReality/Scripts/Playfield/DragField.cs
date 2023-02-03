using Syncreality.Playfield;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragField : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public PlayfieldInteraction playfield;

    public void OnDrag(PointerEventData eventData)
    {
        playfield.dragEvent.Invoke(eventData.pointerCurrentRaycast.worldPosition);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        playfield.tapEvent.Invoke(eventData.pointerCurrentRaycast.worldPosition);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        playfield.releaseEvent.Invoke(eventData.pointerCurrentRaycast.worldPosition);
    }
}
