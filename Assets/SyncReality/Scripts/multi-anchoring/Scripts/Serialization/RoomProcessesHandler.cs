using System;
using UnityEngine;

/// <summary>
/// Component is inherted by all roomProccesses: NewRoomCreator, RoomFromAnotherAppLoader.If you want
/// to add another may of managing the rooms, extend this class.
/// </summary>

public class RoomProcessesHandler : MonoBehaviour
{
    public Action<bool> onProcessEnded;

    public virtual void StartProcess()
    {
    }
    
    protected void SetCentroidPositionByRoomBoundaries(Vector3 centroidPositionByRoomBoundaries)
    {
        SpatialAnchorHandler.Instance.SetPositionOfNewAnchor(centroidPositionByRoomBoundaries);
        SpatialAnchorHandler.Instance.GenerateAnchor();
    }
}
