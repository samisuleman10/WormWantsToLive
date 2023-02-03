using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FingerTapActionSwitch : MonoBehaviour
{
    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnAddMesh;
    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnMoveMesh;
    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnDeleteMesh;

    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnCreateByPointMesh;
    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnCreateByPointBox;
    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnCreateByPointDoor;
    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnCreateByPointWall;

    public UnityEvent<FingerTapCursor.FingerTapEventParameters> OnDefault;




    public void SwitchFingerTapActions(FingerTapCursor.FingerTapEventParameters parameters)
    {
        if (parameters == null)
            return;
        //Debug.Log("In the Switch action");
        //Debug.Log(action.ToString());

        switch (parameters.ActionType)
        {
            case FingerTapActionType.AddMesh:
                OnAddMesh?.Invoke(parameters);
                break;
            case FingerTapActionType.MoveMesh:
                OnMoveMesh?.Invoke(parameters);
                break;
            case FingerTapActionType.DeleteMesh:
                OnDeleteMesh?.Invoke(parameters);
                break;
            case FingerTapActionType.CreateWithPointsMesh:
                OnCreateByPointMesh?.Invoke(parameters);
                break;
            case FingerTapActionType.CreateWithPointsBox:
                OnCreateByPointBox?.Invoke(parameters);
                break;

            case FingerTapActionType.CreateWithPointsDoor:
                OnCreateByPointDoor?.Invoke(parameters);
                break;
            case FingerTapActionType.CreateWithPointsWall:
                OnCreateByPointWall?.Invoke(parameters);
                break;

            default:
                OnDefault?.Invoke(parameters);
                break;
        }
    }
}
