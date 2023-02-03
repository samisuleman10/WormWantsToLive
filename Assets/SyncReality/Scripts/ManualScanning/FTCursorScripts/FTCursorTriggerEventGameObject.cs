using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTCursorTriggerEventGameObject : MonoBehaviour
{
    public FingerTapEvent EventToSend = FingerTapEvent.NoneSelected;

    public void SendBasicCursorEvent()
    {
        FTCursorsEventManager.SendBasicCursorEvent(EventToSend);
    }
}
