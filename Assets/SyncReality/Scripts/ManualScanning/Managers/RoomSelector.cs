
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSelector : MonoBehaviour
{

    private static GameObject CurrentSelectedRoom = null;
    private static GameObject RootScanDataObject = null;

    public GameObject RootGameObject;

    private void Awake()
    {
        CurrentSelectedRoom = null;
        if (RootScanDataObject == null)
        {
            if (RootGameObject != null)
                    RoomSelector.RootScanDataObject = RootGameObject;
            else    RoomSelector.RootScanDataObject = this.gameObject;
        }
    }

    public static GameObject GetRootSpamObj()
    {
        if (RootScanDataObject == null)
        {
            var roomSelector = GameObject.FindObjectOfType<RoomSelector>();
            if (roomSelector == null)
            { 
                Debug.LogError("No RoomSelector behavior in the scene");
                return null;
            }
            return roomSelector.gameObject;
        }
        return RootScanDataObject;
    }
    public static GameObject GetCurrentSelectedRoom()
    {
        if(CurrentSelectedRoom == null)
            CurrentSelectedRoom = CreateNewRoom();

        return CurrentSelectedRoom;
    }

    public static GameObject CreateNewRoom(RoomType type = RoomType.Unselected)
    {
        var root = GetRootSpamObj();
        GameObject newRoom = new GameObject("Room");
        newRoom.transform.SetParent(root.transform);
        var roomGobj = newRoom.AddComponent<RoomTypeGameObject>();
        roomGobj.RoomType = type;

        CurrentSelectedRoom = newRoom;


        return newRoom;
    }
}
