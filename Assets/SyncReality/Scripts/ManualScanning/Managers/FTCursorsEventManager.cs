using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FTCursorsEventManager  
{

    public delegate void ChangeFTCStatus(FingerTapActionType newActionType, Color newColor, ScannedObjectsClassificationType newClassificationStatus = ScannedObjectsClassificationType.NonSelected);
    public delegate void ExitBtnFTCChange(FingerTapActionType newStatus, Collider shape);
    public delegate void SendPosition(GameObject newVector);

    public delegate void ActivateDelegate();
    public delegate void CursorEventDelegate(FingerTapEvent cursorEvent);
    public delegate void FloatDelegate(float newFloat);

    public static ChangeFTCStatus ChangeAllCursorStatus;
    public static SendPosition ReceiveNextTapPosition;
    public static ExitBtnFTCChange ExitBtnFTCEffect;

    public static CursorEventDelegate BasicCursorEventListener;
    public static FloatDelegate ChangeCursorRayDistance;


    public static ActivateDelegate BlockTapsEvent;
    public static ActivateDelegate UnblockTapsEvent;



    public static void ChangeCursorEditRayDistance(float newDistance)
    {
        ChangeCursorRayDistance?.Invoke(newDistance);
    }

    public static void SetNextTapPosition(GameObject nextPosition)
    {
        ReceiveNextTapPosition?.Invoke(nextPosition);
    }

    public static void ChangeFTCActionType(FingerTapActionType newStatus, Color newColor, ScannedObjectsClassificationType newClassificationStatus = ScannedObjectsClassificationType.NonSelected)
    {
        ChangeAllCursorStatus?.Invoke(newStatus, newColor, newClassificationStatus);
    }
    public static void ExitButtonFTCChange(FingerTapActionType newStatus, Collider shape)
    {
        ExitBtnFTCEffect?.Invoke(newStatus, shape);
    }

    public static void SendBasicCursorEvent(FingerTapEvent cursorEvent)
    {
        BasicCursorEventListener?.Invoke(cursorEvent);
    }

    public static void BlockCursorsTap()
    {
        BlockTapsEvent?.Invoke();
    }

    public static void UnblockCursorsTap()
    {
        UnblockTapsEvent?.Invoke();
    }

}
