using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTCursorChangeStatusGameObject : MonoBehaviour
{

    public FingerTapActionType ThisNewFTCAction = FingerTapActionType.None;
    public ScannedObjectsClassificationType ScannedObjectType = ScannedObjectsClassificationType.NonSelected;
    public Color ColorToChangeCursorTo = Color.gray;

    public void ChangeFTCStatus()
    {
        FTCursorsEventManager.ChangeFTCActionType(ThisNewFTCAction, ColorToChangeCursorTo, ScannedObjectType);
    }

    public void ExitBtn()
    {
        FTCursorsEventManager.ExitBtnFTCEffect(ThisNewFTCAction, this.gameObject.GetComponent<Collider>());
    }
}
