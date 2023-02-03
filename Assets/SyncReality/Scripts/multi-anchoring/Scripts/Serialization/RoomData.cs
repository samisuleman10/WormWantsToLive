using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomData
{
    public int id;
    public string RoomName;
    public List<Vector3> RoomObjectsPositions;
    public List<Vector3> RoomBoundariesPositions;
    public List<AnchorIdentifier> anchors;

    public RoomData()
    {
        RoomObjectsPositions = new List<Vector3>();
        RoomBoundariesPositions = new List<Vector3>();
        anchors = new List<AnchorIdentifier>();
    }
}