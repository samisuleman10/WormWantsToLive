using UnityEngine;

/// <summary>
/// Component reads SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG from PlayerPrefs and
/// Starts the proccess of creating a new room or loading an unanchored room. 
/// </summary>

public class ScanningStateMachineManager : MonoBehaviour
{
    [SerializeField] private NewRoomCreator newRoomCreator;
    [SerializeField] private RoomFromAnotherAppLoader unanchoredRoomCreator;

    private void Start()
    {
        var roomLoadingType = RoomLoadingType.NewRoom;

        var hasAnyKey = PlayerPrefs.HasKey(SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG);
        if (hasAnyKey)
        {
            var key = (RoomLoadingType)PlayerPrefs.GetInt(SpatialAnchorUtils.ROOM_LOADING_TYPE_PLAYERPREFS_TAG);
            roomLoadingType = key;
        }

        var anchorId = PlayerPrefs.GetInt(SpatialAnchorUtils.ANCHOR_TO_LOAD_ID_PLAYERPREFS_TAG,-1);

        if (roomLoadingType == RoomLoadingType.NewRoom)
            CreateNewRoom();

        if (roomLoadingType == RoomLoadingType.LoadUnanchoredRoom)
            LoadUnanchoredRoom(anchorId);
    }

    private void CreateNewRoom()
    {
        newRoomCreator.gameObject.SetActive(true);
        newRoomCreator.StartProcess();
        newRoomCreator.onProcessEnded += OnNewRoomProcessEnded;
    }

    private void LoadUnanchoredRoom(int roomId)
    {
        unanchoredRoomCreator.gameObject.SetActive(true);
        unanchoredRoomCreator.StartTheProcess(roomId);
        unanchoredRoomCreator.onProcessEnded += OnNewRoomProcessEnded;
    }

    private void OnNewRoomProcessEnded(bool success)
    {
        if(success)
            Debug.Log("New Room saved succesfully.");
        else
            Debug.Log("New Room not saved.");
    }
}
