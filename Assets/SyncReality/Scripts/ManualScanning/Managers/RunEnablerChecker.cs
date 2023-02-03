using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunEnabler 
{

    public delegate void _delChangeReplacerRunStatus(bool newStatus);

    public static _delChangeReplacerRunStatus ChangeReplacerRunStatusSubscription;

    public static void UpdateReplacerStatus()
    {
        ChangeReplacerRunStatusSubscription?.Invoke(CanEnableRun());
    }

    private static bool CanEnableRun()
    {
        var currentRoom = RoomSelector.GetCurrentSelectedRoom();
        if (currentRoom == null)
            return false;
        return true;
        //Classification of walls are only added on confirmation
        //return currentRoom.GetComponentsInChildren<ScannedTypeGameObject>(true)
          //  .Count(scannedGameObj => scannedGameObj.ObjectClassification == ScannedObjectsClassificationType.Walls) > 0;
    }
}
