using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FingerTapAction
{
    public FingerTapActionType Type;
    public Material TypeLook;

    public static FingerTapAction Default => new FingerTapAction { Type = FingerTapActionType.OtherObject };

}

public enum FingerTapActionType
{
    OtherObject = -4,
    None = -3,
    IndexPoint = -2,
    ConfirmPoint = -1,

    AddMesh = 0,
    MoveMesh = 1,
    RotateMesh = 2,
    DeleteMesh = 3,

    CreateWithPointsMesh = 10,
    CreateWithPointsBox = 11,
    CreateWithPointsDoor = 12,
    CreateWithPointsWall = 13,

    EditMesh = 20

    //MoveMeshPlane = 3,
    //MoveMeshPoint = 4,  
    //AddMeshPoint = 5
}


public enum FingerTapEvent
{
    NoneSelected = -20,

    Tap = 0,
    Hold = 1,
    Release = 2,
    Confirm = 3,
    Cancel = 4,
    Select = 5,
    Deselect = 6,
    Undo = 7,

    ActivateEdit = 22
    //SecondarySelect = 15

}
